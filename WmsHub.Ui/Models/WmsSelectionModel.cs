using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace WmsHub.Ui.Models
{
    public class WmsSelectionModel
    {
        [Required(ErrorMessage = "*A service must be selected")]
        public string WmsName { get; set; }

        public List<SelectListItem> WmsList { get; set; }
    }
}