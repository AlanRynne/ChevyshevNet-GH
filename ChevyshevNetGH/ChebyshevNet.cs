using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace ChevyshevNetGH
{
    public class ChebyshevNet
    {
        //Class level private properties
        Surface _surface;
        Point3d _startingPoint;
        double _desiredLength;
        DataTree<Line> _warpNet;
        DataTree<Line> _weftNet;
        DataTree<Point3d> _grid;
        int _MAXITERATIONS = 1000;
        double _angle; // In radians!!
        Boolean _extend;
        double _axisNum;


        //Class level Public properties
        //public Surface Surface { get; set; }
        //public Point3d StartingPoint { get; set; }
        //public double DesiredLength { get; set; }
        public DataTree<Line> WarpNet { get { return _warpNet; }} // Read only value
        public DataTree<Line> WeftNet { get { return _weftNet; }} // Read only value
        public DataTree<Point3d> Grid {get { return _grid; }} // Read only value

        //Constructor
        public ChebyshevNet(Surface aSurface, Point3d aStartingPoint, double aDesiredLength, double angleInRad, bool extend, double numberOfAxis)
        {
            _surface = aSurface;
            _startingPoint = aStartingPoint;
            _desiredLength = aDesiredLength;
            _angle = angleInRad;
            _extend = extend;
            _grid = new DataTree<Point3d>();
            _warpNet = new DataTree<Line>();
            _weftNet = new DataTree<Line>();
            _axisNum = numberOfAxis;
        }

        //Methods
        public void GenerateChebyshevNet()
        { // Main method for grid generation

            // Create empty placeholder trees
            DataTree<Point3d> gridAxisPoints = new DataTree<Point3d>();
            DataTree<Point3d> gridPoints = new DataTree<Point3d>();

            // Extend surface beyond boundaries to get a better coverage from the net
            if (_extend)
            {
                _surface = _surface.Extend(IsoStatus.North, _desiredLength * 2, true);
                _surface = _surface.Extend(IsoStatus.East, _desiredLength * 2, true);
                _surface = _surface.Extend(IsoStatus.South, _desiredLength * 2, true);
                _surface = _surface.Extend(IsoStatus.West, _desiredLength * 2, true);
            }

            // Find starting point u,v and tangent plane
            double u, v;
            _surface.ClosestPoint(_startingPoint, out u, out v); // Make sure the point is in the surface
            Point3d stPt = _surface.PointAt(u, v);
            Vector3d n = _surface.NormalAt(u, v);
            Plane tPlane = new Plane(stPt, n);

            //Rotate vector
            tPlane.Rotate(_angle, tPlane.ZAxis);

            // Set direction list
            List<Vector3d> dir = new List<Vector3d>();
            for (int axisCount = 0; axisCount < _axisNum; axisCount++){
                double rotation = ((2 * Math.PI) / _axisNum) * axisCount;
                Vector3d thisAxis = tPlane.XAxis;
                thisAxis.Rotate(rotation, tPlane.ZAxis);
                dir.Add(thisAxis);
                
            }
            //dir.Add(tPlane.XAxis * _desiredLength);
            //dir.Add(tPlane.YAxis * _desiredLength);
            //dir.Add(tPlane.XAxis * - _desiredLength);
            //dir.Add(tPlane.YAxis * - _desiredLength);

            // Generate Axis Points for Net
            gridAxisPoints = findAllAxisPoints(_startingPoint, dir);

            // Generate the Grid
            gridPoints = getAllGridPoints(gridAxisPoints);


            //Assign values to class variables
            _grid = gridPoints;
            //CleanGrid();
            //_net = gridLines;
        }

        void CleanGrid()
        {
            DataTree<Point3d> cleanTree = new DataTree<Point3d>();

            foreach (GH_Path path in _grid.Paths)
            {
                int quadrantIndex = path.Indices[0];

                if (quadrantIndex == 0)
                {
                    
                }
                else if (quadrantIndex == 1) 
                {
                    
                }
                else if (quadrantIndex == 2)
                {
                    
                } 
                else if (quadrantIndex == 3)
                {
                    
                }
            }

            _grid = cleanTree;

        }

        DataTree<Point3d> getAllGridPoints(DataTree<Point3d> axisPoints)
        { // Assigns to '_grid' a tree with as many ranches as items contained in the gridAxisList

            DataTree<Point3d> resultingPoints = new DataTree<Point3d>();

            for (int i = 0; i < axisPoints.BranchCount; i++)
            { // Iterate on all axises
                DataTree<Point3d> quarterGrid = new DataTree<Point3d>();
                List<Point3d> xAxis;
                List<Point3d> yAxis;

                if(i%2==0){
                    xAxis = axisPoints.Branch(new GH_Path(i+1));
                    yAxis = axisPoints.Branch(new GH_Path(i));
                    if (i == axisPoints.BranchCount - 1)
                    {
                        xAxis = axisPoints.Branch(new GH_Path(0));
                        yAxis = axisPoints.Branch(new GH_Path(i));
                    }
                }
                else
                {
                    xAxis = axisPoints.Branch(new GH_Path(i));
                    yAxis = axisPoints.Branch(new GH_Path(i + 1));
                    if (i == axisPoints.BranchCount - 1)
                    {
                        xAxis = axisPoints.Branch(new GH_Path(i));
                        yAxis = axisPoints.Branch(new GH_Path(0));
                    }
                }


                // Fill x and y axis list and wrap in the last index


                int[] complexPath = new int[] { i, 0 };
                quarterGrid.AddRange(xAxis, new GH_Path(complexPath)); //Add xAxis to path 0 of the quarter

                for (int j = 1; j < yAxis.Count; j++)
                { // Iterate on all yAxis Points EXCEPT the first one
                    complexPath = new int[] { i, j };
                    Point3d lastPoint = yAxis[j];
                    quarterGrid.Add(lastPoint,new GH_Path(complexPath)); //Add yAxis Point to list

                    for (int k = 1; k < xAxis.Count; k++)
                    { // Iterate on all xAxis Points EXCEPT the first one

                        //Intersection!!!
                        Sphere sphere1 = new Sphere(lastPoint, _desiredLength);
                        Sphere sphere2 = new Sphere(xAxis[k], _desiredLength);
                        Circle cir1;
                        Intersection.SphereSphere(sphere1, sphere2, out cir1);
                        CurveIntersections crvint = Intersection.CurveSurface(cir1.ToNurbsCurve(), _surface, 0.001, 0.001);

                        if (crvint.Count <= 1)
                        { // If  one or 0 intersections are found BREAK
                            break;
                        }
                        else 
                        { // If 2 points are found, filter by distance to diagonal point
                            double u, v;
                            foreach(IntersectionEvent iE in crvint)
                            {
                                 
                                iE.SurfacePointParameter(out u, out v);

                                Point3d tmpPt = _surface.PointAt(u, v);
                                //int[] diagPath = new int[] { i, j - 1 };
                                //Point3d diagPt = quarterGrid[new GH_Path(diagPath), k - 1];
                                double dist = tmpPt.DistanceTo(xAxis[k-1]);
                                if (dist < 0.02) 
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    quarterGrid.Add(tmpPt, new GH_Path(complexPath));
                                    lastPoint = tmpPt;
                                    break;
                                }
                            }
                        } 
                    }
                    xAxis = quarterGrid.Branch(complexPath);
                }
                resultingPoints.MergeTree(quarterGrid);
                // Generate net using Grid
                createNetFromPoints(quarterGrid);
            }
            return resultingPoints;
        }

        void createNetFromPoints(DataTree<Point3d> PointGrid) 
        {
            // Receives a tree of points and gives back it's corresponding net of lines properly divided into WARP AND WEFT directions
            DataTree<Line> warpLines = new DataTree<Line>();
            DataTree<Line> weftLines = new DataTree<Line>();

            //WARP
            for (int bNum = 0; bNum < PointGrid.BranchCount; bNum++)
            { // Iterate all branches
                List<Point3d> branch = PointGrid.Branches[bNum];
                GH_Path pth = PointGrid.Paths[bNum];
                for (int ptNum = 0; ptNum < branch.Count - 1; ptNum++)
                { // Iterate all points in each branch
                    Line warpLn = new Line(branch[ptNum], branch[ptNum + 1]);
                    warpLines.Add(warpLn, new GH_Path(pth));
                    if (bNum < PointGrid.BranchCount - 1)
                    {
                        List<Point3d> nextBranch = PointGrid.Branches[bNum + 1];
                        if (ptNum < nextBranch.Count)
                        {
                            Line weftLn = new Line(branch[ptNum], nextBranch[ptNum]);
                            weftLines.Add(weftLn, pth);
                        }
                    }
                }
            }

            _warpNet.MergeTree(warpLines);
            _weftNet.MergeTree(weftLines);
        }

        DataTree<Point3d> findAllAxisPoints(Point3d startP, List<Vector3d> directions)
        { 
            /// 'Walk out' from the center using a list of directions to find all points in this surface 'axis'
            /// Will output a tree with as many branches as directions where input

            /// MAIN BEHAVIOUR
            /// Create an arc using the normal, the direction and the negative normal of size DesiredLength
            /// Intersect the arc with the surface to find next point.
            /// After finding the next point, update current u,v values and current Direction
            /// If no intersections are found BREAK: You have reached the limit of the surface

            DataTree<Point3d> axis = new DataTree<Point3d>(); //Create an empty array of List<Point3d>

            for (int i = 0; i < _axisNum; i++)
            { // Iterate for every axis

                List<Point3d> pts = new List<Point3d>();
                double u0, v0;
                Vector3d d = directions[i]; // Set direction to starting dir

                _surface.ClosestPoint(startP, out u0, out v0); // Get U,V of the startingPoint

                double u = u0;
                double v = v0;

                for (int j = 0; j < _MAXITERATIONS; j++)
                { // Iterate until no intersections or maxIterations is reached


                    // Get the current point and normal 
                    Point3d pt = _surface.PointAt(u, v);
                    Vector3d n = _surface.NormalAt(u, v);

                    pts.Add(pt); // Add the point to the list
                    n *= _desiredLength; // Set n length to desired
                    d.Unitize(); // Make shure d is unitary
                    d *= _desiredLength; // Set d lenght to desired

                    Arc intArc = new Arc(pt + n, pt + d, pt - n);

                    CurveIntersections cvint =
                        Intersection.CurveSurface(intArc.ToNurbsCurve(), _surface, 0.01, 0.01); // Intersect arc with geometry

                    if (cvint.Count > 0)
                        cvint[0].SurfacePointParameter(out u, out v); // Find u,v of intersection point
                    else
                        break; // Break if no intersections are found

                    d = _surface.PointAt(u, v) - pt; // Update direction
                }

                axis.AddRange(pts,new GH_Path(i)); // Add axis points list to branch
            }
            return axis; //Return the axis points of the grid
        }
    }
}
