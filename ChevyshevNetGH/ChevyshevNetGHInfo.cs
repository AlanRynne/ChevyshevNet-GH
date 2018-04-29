using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace ChevyshevNetGH
{
    public class ChevyshevNetGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Chevyshev Net";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Creates a grid of same length in any given freeform surface";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("02c218f5-b73e-4019-9718-e706aebb1912");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Alan Rynne";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "www.rynne.es || alan@rynne.es";
            }
        }
    }
}
