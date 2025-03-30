using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public enum DType
    {
        [Display(Name = "Acquisition")]
        Acquisition = 1,

        [Display(Name = "Dropoff")]
        Dropoff = 2
    }

    public enum DStatus
    {
        [Display(Name = "Active")]
        Active = 1,

        [Display(Name = "Offline")]
        Offline = 2,

        [Display(Name = "Broken")]
        Broken = 3
    }
}