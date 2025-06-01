using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    
    public enum DStatus
    {
        [Display(Name = "Active")]
        Active = 1,

        [Display(Name = "Offline")]
        Offline = 2,

        [Display(Name = "Broken")]
        Broken = 3,

        [Display(Name = "Busy")]
        Busy = 4
    }
}