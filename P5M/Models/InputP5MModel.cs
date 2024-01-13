using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class InputP5MModel
    {
        public string nim { get; set; }
        public Dictionary<int, bool> CheckboxValues { get; set; }
    }


}
