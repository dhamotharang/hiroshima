using System.Collections.Generic;
using System.Linq;
using FluentValidation.Validators;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Common.Models.Accommodations;

namespace HappyTravel.Hiroshima.DirectManager.RequestValidators
{
    public class MultiLanguageValidator : PropertyValidator
    {
       protected override bool IsValid(PropertyValidatorContext context)
       {
           switch (context.PropertyValue)
           {
               case null:
               case MultiLanguage<string> str when str.GetValues().Any():
               case MultiLanguage<TextualDescription> textualDescription when textualDescription.GetValues().Any():
               case MultiLanguage<List<string>> strList when strList.GetValues().Any():
               case MultiLanguage<List<Models.Requests.Room>> roomList when roomList.GetValues().Any():
                   return true;
               
               default: return false;
           }
       }

       
       public MultiLanguageValidator(string error) : base(error)
       {}
    }
}