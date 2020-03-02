using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Spice.Extensions
{
    public static class IEnumerableExtension
    {
        //converts an object into select list item ienumerable 
        public static IEnumerable<SelectListItem> ToSelectListItem<T> (this IEnumerable<T> items, int selectedValue)
        {
            return from item in items
                   //this creates the options tag for the categorises available
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Name"),//retrieves the value of the property "Name" in one category object
                       Value = item.GetPropertyValue("Id"),//retrieves the value of the property "Id" in the same category object
                       //this sets the selected attr to true
                       Selected = item.GetPropertyValue("Id").Equals(selectedValue)//retrieves the value of the property "Id" in one category object and Checks if it equals a selected values
                   };
        }
    }
}
