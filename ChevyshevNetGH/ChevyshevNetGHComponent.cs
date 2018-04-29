using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ChevyshevNetGH
{
    public class ChevyshevNetGHComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ChevyshevNetGHComponent()
          : base("CompassMethod", "CMethod",
                 "CompassMethod to obtain a same length quad grid on any given surface (has some exceptions)",
                 "Alan", "Gridshells")
        {
            /// <summary>
            /// This is the constructor of the component!
            /// Custom class variables should be initialized here to avoid early initialization
            /// when Grasshopper starts.
            /// </summary>

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Srf", "Surface on which to obtain the grid", GH_ParamAccess.item);
            pManager.AddPointParameter("Starting Point", "P", "Starting UV Coordinates for grid", GH_ParamAccess.item);
            pManager.AddNumberParameter("Grid Size", "L", "Specify grid size for Chebyshev net", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Rotation Angle", "Angle", "Rotation angle in radians", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Extend Surface", "Extend", "Set to true to extend the surface", GH_ParamAccess.item, true);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Chebyshev Point Grid", "PtGrid", "To be written....", GH_ParamAccess.tree);
            pManager.AddLineParameter("Warp Net direction", "Warp", "Resulting warp direction net of the Compass Method algorithm", GH_ParamAccess.tree);
            pManager.AddLineParameter("Weft Net direction", "Weft", "Resulting weft direction net of the Compass Method algorithm", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // DECLARE PRIVATE VARIABLES
            Surface surf = null;
            if (!DA.GetData(0, ref surf)) { return; }

            Point3d stPt = Point3d.Unset;
            if (!DA.GetData(1, ref stPt)) { return; }

            double gridLength = 1.0;
            if (!DA.GetData(2, ref gridLength)) { return; }

            double rotationAngle = 0.0;
            if (!DA.GetData(3, ref rotationAngle)) { return; }

            bool surfExtend = true;
            if (!DA.GetData(4, ref surfExtend)) { return; }

            // DO CHEBYSHEV HERE!!
            ChebyshevNet net = new ChebyshevNet(surf, stPt, gridLength, rotationAngle, surfExtend);
            net.GenerateChebyshevNet();
            //DataTree<Point3d> tree = new DataTree<Point3d>();
            // OUTPUT DATA (MUST BE GH_TREE)
            DA.SetDataTree(0, net.Grid);
            if (net.WarpNet != null) DA.SetDataTree(1, net.WarpNet);
            if (net.WeftNet != null) DA.SetDataTree(2, net.WeftNet);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;

                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a6b18f52-cff2-49e2-850f-e3b3f91bf0d6"); }
        }
    }
}
