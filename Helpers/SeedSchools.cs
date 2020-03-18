using System.Collections.Generic;
using System.IO;
using System.Linq;
using Excel_Accounts_Backend.Data;
using Excel_Accounts_Backend.Models;

namespace Excel_Accounts_Backend.Helpers
{
    public class RootObj
    {
        public List<School> Schools { get; set; }
    }
    public class SeedSchools
    {
        // Extract data from a json file ("Schoollist.json)
        // And populate the table - Schools
        public static void SeedData(DataContext context)
        {
            if (!context.Schools.Any())
            {
                string jsonStirng = File.ReadAllText("Schoollist.json");
                var root = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObj>(jsonStirng);
                foreach (var item in root.Schools)
                {
                    var school = new School();
                    school.Name = item.Name;                   
                    context.Schools.Add(school);
                    context.SaveChanges();
                }
            }

        }    
    }
}