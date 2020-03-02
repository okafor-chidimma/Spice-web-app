using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Extensions
{
    public static class ReflectionExtension
    {
        public static string GetPropertyValue<T> (this T item, string propertyName)
        {
            //var myType = item.GetType();
            //var myProp = myType.GetProperty(propertyName);
            //var myValue = myProp.GetValue(item, null);


            //this is an extension method which means that this is a method that can be used on a type that was not defined as a method in the class
            //or struct when that class was defined
            //it is a static method but called on a type as if it is an instance method

            //this line of code does is this
            //1 Gets the type of what ever type item is since the extension method is done on item
            //2. Checks if this item has a certain property with same name as the 2nd parameter passed in
            //3. if yes, gets the Value of the property
            //4 converts it to string

            return item.GetType().GetProperty(propertyName).GetValue(item, null).ToString();
        }
    }
}
