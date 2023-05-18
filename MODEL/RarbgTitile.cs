using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL
{
    public class RarbgTitile
    {

        int id;

        string name;

        string maglink;
        
        float size;

        string path;



        public RarbgTitile()
        {
        }

        public RarbgTitile(int id, string name, string maglink, float size )
        {
            this.id = id;
            this.name = name;
            this.maglink = maglink;
            this.size = size;
        }

        public string Maglink { get => maglink; set => maglink = value; }
        public string Name { get => name; set => name = value; }
        public int Id { get => id; set => id = value; }

        string filteredName;

        public string FilteredName
        {
            get
            {
                if (String.IsNullOrEmpty(filteredName))
                {
                    filteredName = Tool.filterName(name);
                }
                return filteredName;
            }
        }
        public float Size { get => size; set => size = value; }
        public string Path { get => path; set => path = value; }
    }
}
