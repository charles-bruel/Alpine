import random
import math
import matplotlib.pyplot as plt
from cProfile import Profile
from pstats import SortKey, Stats
import functools
   
class Spline:
    """
    Implements a basic NURBS curve
    """
    def __init__(self, points, degree):
        self.points = points
        self.knots = list(range(0, len(points) + degree + 1))
        self.weights = [1] * len(points)
        self.degree = degree

    def clone(self):
        return Spline(list(self.points), self.degree)

    def get_points(self):
        return self.points
    
    def set_points(self, points):
        self.points = points

    @functools.cache
    def basis_fun(self, i, n):
        @functools.cache
        def N_in(u):
            if n == 0:
                if self.knots[i] <= u < self.knots[i + 1]:
                    return 1
                else:
                    return 0
            else:
                f_in = (u - self.knots[i]) / (self.knots[i + n] - self.knots[i])
                g_in1 = (self.knots[i + n + 1] - u) / (self.knots[i + n + 1] - self.knots[i + 1])
                return f_in * self.basis_fun(i, n - 1)(u) + g_in1 * self.basis_fun(i + 1, n - 1)(u)
        return N_in

    def get_curve(self):
        basis_functions = []
        for i in range(len(self.points)):
            basis_functions.append(self.basis_fun(i, self.degree))
        def C(u):
            # xtotal = 0
            # ytotal = 0
            # for i in range(len(self.points)):
            #     num = basis_functions[i](u) * self.weights[i]
            #     dom = 0
            #     for j in range(len(self.points)):
            #         dom += basis_functions[j](u) * self.weights[j]
            #     xtotal += num/dom * self.points[i][0]
            #     ytotal += num/dom * self.points[i][1]
            xnum = 0
            ynum = 0
            xdom = 0
            ydom = 0
            for i in range(len(self.points)):
                xnum += basis_functions[i](u) * self.weights[i] * self.points[i][0]
                ynum += basis_functions[i](u) * self.weights[i] * self.points[i][1]
                xdom += basis_functions[i](u) * self.weights[i]
                ydom += basis_functions[i](u) * self.weights[i]
            return (xnum / xdom, ynum / ydom)
        return C

    def evaluate(self, n):
        """
        Evaluate the spline at n points across the whole range.
        """
        C = self.get_curve()
        u_0 = 1
        u_max = self.knots[-1]
        step = (u_max - u_0) / n
        xvals = []
        yvals = []
        for i in range(n):
            point = C(u_0 + i * step)
            xvals.append(point[0])
            yvals.append(point[1])
        return (xvals, yvals)

def generate_data(n):
    angle = random.random() * 2 * math.pi
    xvals = []
    yvals = []
    x, y = 0, 0
    while len(xvals) < n:
        flag = False
        if random.random() < abs(math.cos(angle)):
            flag = True
            x += math.copysign(1, math.cos(angle))
        if random.random() < abs(math.sin(angle)):
            flag = True
            y += math.copysign(1, math.sin(angle))
        if not flag:
            continue
        angle += (random.random() - 0.5) * 2 * math.pi * 0.05
        xvals.append(x)
        yvals.append(y)
    return (xvals, yvals)

def create_spline(xvals, yvals, n):
    # Take n points evenly from xvals and yvals and use those as the initial control points
    xpoints = []
    ypoints = []
    for i in range(n-1):
        xpoints.append(xvals[int(i / (n - 1) * len(xvals))])
        ypoints.append(yvals[int(i / (n - 1) * len(yvals))])
    xpoints.append(xvals[-1])
    ypoints.append(yvals[-1])
    return Spline(list(zip(xpoints, ypoints)), 2)

def calculate_r2(data, splines, stride=1):
    """
    Takes the average of the minimum square distance between points on the spline
    and the data.
    """
    
    total = 0
    j = 0
    for (i, data_point) in enumerate(zip(data[0], data[1])):
        if i % stride != 0:
            continue
        last_r2 = math.inf
        while True:
            r2 = (splines[0][j] - data_point[0]) ** 2 + (splines[1][j] - data_point[1]) ** 2
            if r2 < last_r2:
                last_r2 = r2
                j += 1
            else:
                # If it starts getting biggest, the last one was the smallest
                j -= 1
                break
        total += last_r2
    return total * stride / len(data)


def gradient_descent(spline, data, iterations, step=0.1, pct_threshold=1, evaluation_size=0.1):
    """
    Performs gradient descent on the spline to minimize the error between the spline
    and the data.
    """
    working_spline = spline.clone()
    evaluation_size = int(evaluation_size * len(data[0]))
    for iter in range(iterations):
        base_r2 = calculate_r2(data, working_spline.evaluate(evaluation_size))
        spline_points = working_spline.points
        
        # Save the points in case this iteration makes it worse
        base_spline_points = list(spline_points)

        dr2dkix = []
        dr2dkiy = []
        # Calculate the partial derivatives of moving the control points in each direction
        # TODO: Stochastic gradient descent
        for i in range(len(working_spline.get_points())):
            # Try x axis
            spline_points[i] = (spline_points[i][0] + step, spline_points[i][1])
            working_spline.set_points(spline_points)
            dr2dkix.append((calculate_r2(data, working_spline.evaluate(evaluation_size)) - base_r2) / step)
            # Try y axis
            spline_points[i] = (spline_points[i][0] - step, spline_points[i][1] + step)
            working_spline.set_points(spline_points)
            dr2dkiy.append((calculate_r2(data, working_spline.evaluate(evaluation_size)) - base_r2) / step)
            # Reset
            spline_points[i] = (spline_points[i][0], spline_points[i][1] - step)

        # Calculate the gradient by normalizing the vector of partial derivatives
        norm = 0
        for i in range(len(dr2dkix)):
            norm += dr2dkix[i] ** 2 + dr2dkiy[i] ** 2
        norm = math.sqrt(norm)
        dr2dkix = [x / norm for x in dr2dkix]
        dr2dkiy = [y / norm for y in dr2dkiy]

        # Move the control points in the direction opposite to the gradient
        # Opposite direction decreases error
        # Variable step size; multiply by 1.5 until the r2 stops decreasing
        step_size_multiplier = 1
        last_r2 = base_r2
        last_spline_points = list(spline_points)
        while True:
            last_spline_points = list(spline_points)
            for i in range(len(working_spline.get_points())):
                spline_points[i] = (spline_points[i][0] - dr2dkix[i] * step * step_size_multiplier, spline_points[i][1] - dr2dkiy[i] * step * step_size_multiplier)
            new_r2 = calculate_r2(data, working_spline.evaluate(evaluation_size))
            # Reset
            if new_r2 > last_r2:
                spline_points = last_spline_points
                break
            last_r2 = new_r2
            step_size_multiplier *= 1.5
        
        pct_difference = 100 * (base_r2 - last_r2) / last_r2
        print(str(iter) + ": " + str(pct_difference))
        # If we're not improving by at least pct_threshold, stop
        # If last_r2 (which is the more recent one) is larger than base,
        # we're negative so we want to stop and use the last one
        if last_r2 > base_r2:
            working_spline.set_points(base_spline_points)
            break
    
        working_spline.set_points(spline_points)
        if pct_difference < pct_threshold:
            break

    return working_spline

def graph(data, splines):
    fig = plt.figure()
    ax = fig.add_subplot()
    ax.plot(data[0], data[1])
    for spline in splines:
        ax.plot(spline[0], spline[1])
    ax.set_aspect('equal')
    plt.show()

if __name__ == "__main__":
    # random.seed(0)
    data, spline1_result, spline2_result = None, None, None
    with Profile() as profile:
        data = generate_data(1000)
        spline = create_spline(data[0], data[1], 20)
        spline2 = gradient_descent(spline, data, 100, pct_threshold=0.5, step=0.1)
        spline1_result = spline.evaluate(500)
        spline2_result = spline2.evaluate(500)
        (
            Stats(profile)
            .strip_dirs()
            .sort_stats(SortKey.CUMULATIVE)
            .print_stats()
        )
    print(calculate_r2(data, spline2_result, 1))
    graph(data, [spline1_result, spline2_result])