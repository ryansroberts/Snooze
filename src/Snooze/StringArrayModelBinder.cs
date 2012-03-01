using System.Web.Mvc;

namespace Snooze
{
    public class StringArrayModelBinder : DefaultModelBinder
    {
        protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, object value)
        {
            //is this is a string array, with comma seperated values in the 1st element
            if (propertyDescriptor.PropertyType == typeof(string[]) && !string.IsNullOrEmpty(((string[])(value))[0]) && ((string[])(value))[0].Contains(","))
            {
                var newValue = ((string[]) (value))[0].Split(new char[] {','});

                base.SetProperty(controllerContext, bindingContext, propertyDescriptor, newValue);        
            }
            else
                base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);        
        }
    }
}