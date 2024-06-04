//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurableServiceProvider : ServiceProvider {

    [Header("Building Information")]
    public bool HasBathroom;
    public int Slots;
    public float Volume;

    [Header("Configuration Information")]
    public List<ServiceInformation> CurrentServices = new List<ServiceInformation>();

    public override Service[] Services() {
        List<Service> services = new List<Service>();
        foreach(var info in CurrentServices) {
            foreach(var service in info.Service) {
                services.Add(service);
            }
        }
        if(HasBathroom) {
            // TODO: Extract to somewhere else for easy configuration
            services.Add(new Service(Need.BATHROOM, 1f, 5));
        }
        return services.ToArray();
    }
}
