public static class Extensions {
    //Writing this as an extension method allows it to be called on a null
    //This is very convenient as we already have nice neat null behavior
    public static T Fetch<T>(this APIDef APIDef) where T : APIBase {
        T toReturn = (T) ReflectionHelper.GetInstance(typeof(T), APIDef);

        if(APIDef == null) return toReturn;

        toReturn.FloatParameters      = APIDef.Params.FloatParameters;
        toReturn.IntParameters        = APIDef.Params.IntParameters;

        return toReturn;
    }
}