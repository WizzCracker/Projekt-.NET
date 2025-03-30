using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public enum DelivType
    {
        Acquisition = 1,
        Dropoff = 2
    }

    public enum Status
    {
        Active = 1,
        Offline = 2,
        Broken = 3
    }
}