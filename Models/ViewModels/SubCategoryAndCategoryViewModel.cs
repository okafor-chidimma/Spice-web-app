using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


//view model is used to hold entities, listed out as properties that we want to display in a view
//all these properties are not already in a model. this is just a combination of different models or other things that we want to use on a view

namespace Spice.Models.ViewModels
{
    public class SubCategoryAndCategoryViewModel
    {
        //for a list of category including name and id
        public IEnumerable<Category> CategoryList { get; set; }

        //for us to use tag helpers such asp-for
        public SubCategory SubCategory { get; set; }

        //to hold a list of subcategories, did not use ienum because I just want the Name
        public List<string> SubCategoryList { get; set; }

        //to hold the status message
        public string StatusMessage { get; set; }
    }
}
