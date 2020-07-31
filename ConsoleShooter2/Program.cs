using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;

namespace ConsoleShooter2
{
    /*
    * Set Console Font to Consolas
    * The Unit Circle turns clockwise because the coordinate system is flipped, 
    * so looking "up" to the top of the screen is 270° instead of 90°
    * 1, 1, 1, 1,          ^
    * 1, 0, 0, 1,         / \
    * 1, 0, 0, 1,        /   \
    * 1, 0, 0, 1,          |
    * 1, 0, 0, 1,          |      
    * 1, 0, 0, 1,   /------| 
    * 1, 0, 0, 1,   |      |__________
    * 1, 0, 0, 1,   | 270°    | 
    * 1, 1, 1, 1,   \---------/
    */
    class Program
    {
        static int framerate = 60;
        static int frameCount = 0;
        class HUD
        {
            public static int lastFrameCount = 0;
            public static bool IsActive = false;
            public static void Activate()
            {
                if (frameCount - lastFrameCount > framerate)
                {
                    IsActive = (IsActive == false) ? true : false;
                    lastFrameCount = frameCount;
                }
            }
        }

        class Player
        {
            public PointF Position = new PointF(1, 13.9f);
            private float _fAngle = 270;
            public float fAngle
            {
                get { return _fAngle; }
                set
                {
                    if (value >= 360) // If integer part of float value is 360, then convert to 0
                        _fAngle = value - 360; // Preserve floating point
                    else if (value < 0) // If integer part of float value is -1, then convert to 359
                        _fAngle = value + 360; // Preserve floating point
                    else
                        _fAngle = value;
                }
            }

            public float fSpeed = 0.07f;
            public float fFOV = 45;

        }
        class PointF
        {
            public float X;
            public float Y;
            public PointF(float X, float Y)
            {
                this.X = X;
                this.Y = Y;
            }

            public void MoveAtAngle(float degrees, float distance)
            {
                // Convert to radians
                double radians = degrees * Math.PI / 180;
                // Get Unit Vector
                double x = Math.Cos(radians);
                double y = Math.Sin(radians);
                // Get absolute vector
                x *= distance;
                y *= distance;
                // Add vector onto this PointF instance
                X += (float)x;
                Y += (float)y;
            }

            public double GetLength()
            {
                double length = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
                return length;
            }

            /// <summary>
            /// Returns angle in degrees from 0 to 359
            /// </summary>
            /// <returns></returns>
            public double GetDegrees()
            {
                double radians = Math.Atan(Y / X);
                double degrees = radians / Math.PI * 180;
                if (degrees >= 360)
                    degrees -= 360;
                else if (degrees < 0)
                    degrees += 360;

                return degrees;
            }

            public PointF GetUnitVector()
            {
                PointF unitVector = new PointF((float)(X/GetLength()), (float)(Y/GetLength()));
                return unitVector;
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            // Set Console dimensions
            Console.OutputEncoding = Encoding.Unicode;
            Console.SetBufferSize(120, 30);
            int consoleBufferWidth = Console.BufferWidth; // To prevent slowdown when accessing these properties
            int consoleBufferHeight = Console.BufferHeight;
            StringBuilder outputBuilder = new StringBuilder(consoleBufferHeight * consoleBufferWidth);
            int[] colorBuffer;
            Console.CursorVisible = false;
            double drawDistance = 11.5;
            Player player = new Player();

            // Dot product test
            PointF v1 = new PointF(1, 1);
            Debug.WriteLine("v1 degrees: " + v1.GetDegrees());
            PointF v2 = new PointF(1, 0);
            Debug.WriteLine("v2 degrees: " + v2.GetDegrees());
            float dP = v1.X * v2.X + v1.Y * v2.Y;
            Debug.WriteLine("dotProduct: " + dP);


            // Wall representations based on distance
            char[] wallPieces = new char[] { '█', '▓', '▒', '░' };
            //char[] wallPieces = new char[] { '█', '▇', '▆', '▓', '▒', '░' };
            char[] groundPieces = new char[3] { 'x', '.', '_' };
            int[,] map = new int[15, 20]
            {
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1 },
                { 1,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1 },
                { 1,0,1,1,1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1 },
                { 1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1 },
                { 1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1 },
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
            };

            // Game loop
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                if (watch.ElapsedMilliseconds < 1000 / framerate)
                    continue;

                watch.Restart();
                // Controls
                PointF playerSave = new PointF(player.Position.X, player.Position.Y);
                if (Keyboard.IsKeyDown(Key.Left))
                    player.Position.MoveAtAngle(player.fAngle - 90, player.fSpeed);
                if (Keyboard.IsKeyDown(Key.Right))
                    player.Position.MoveAtAngle(player.fAngle + 90, player.fSpeed);
                if (Keyboard.IsKeyDown(Key.Up))
                    player.Position.MoveAtAngle(player.fAngle, player.fSpeed);
                if (Keyboard.IsKeyDown(Key.Down))
                    player.Position.MoveAtAngle(player.fAngle - 180, player.fSpeed);
                if (Keyboard.IsKeyDown(Key.A))
                    player.fAngle -= 1.3f;
                if (Keyboard.IsKeyDown(Key.D))
                    player.fAngle += 1.3f;
                if (Keyboard.IsKeyDown(Key.Enter))
                    HUD.Activate();

                // Out of bounds/collision check -> Reset
                if (player.Position.X > map.GetLength(1) - 1 || player.Position.X < 0 ||
                    player.Position.Y > map.GetLength(0) - 1 || player.Position.Y < 0 ||
                    map[(int)player.Position.Y, (int)player.Position.X] != 0)
                    player.Position = new PointF(playerSave.X, playerSave.Y);

                // Prepare output builder
                outputBuilder.Clear();
                colorBuffer = new int[consoleBufferHeight * consoleBufferWidth];
                outputBuilder.Append(' ', consoleBufferHeight * consoleBufferWidth);

                // Raycasting
                for (int column = 0; column < 120; column++) // cycle through 120 rays
                {
                    // Get angle for this ray
                    float fRayAngle = player.fAngle - (player.fFOV / 2) + (column * player.fFOV / 120);
                    // Convert degrees to Radians
                    double dRayAngle_radians = fRayAngle * Math.PI / 180;
                    // Get unit vector from rayAngle
                    PointF unitVector = new PointF((float)Math.Cos(dRayAngle_radians), (float)Math.Sin(dRayAngle_radians));
                    float distanceFactor = 0; // Smoother scan with float double?
                    // Advance the ray forwards
                    while (true)
                    {
                        distanceFactor += 0.1f;
                        PointF rayPoint_player = new PointF(unitVector.X * distanceFactor, unitVector.Y * distanceFactor);
                        PointF rayPoint_world = new PointF(player.Position.X + rayPoint_player.X, player.Position.Y + rayPoint_player.Y);
                        // Check if raypoint still in bounds of map
                        if (rayPoint_world.Y < map.GetLength(0) && rayPoint_world.X < map.GetLength(1) &&
                            rayPoint_world.Y >= 0 && rayPoint_world.X >= 0)
                        {
                            // Check if ray is beyond draw distance -> makes black column
                            double distance = rayPoint_player.GetLength();
                            if (distance >= drawDistance)
                                break;
                            // Check if wall has been hit
                            PointF wallCoords = new PointF((int)rayPoint_world.X, (int)rayPoint_world.Y);
                            if (map[(int)wallCoords.Y, (int)wallCoords.X] == 1)
                            {
                                // If ray has hit corner of block, don't draw wall
                                bool hitCorner = false;
                                    
                                // Get Vectors pointing from player to each wall corner (single vectors always begin at origin)
                                List<PointF> cornerVectors = new List<PointF> {
                                    new PointF(wallCoords.X - player.Position.X, wallCoords.Y - player.Position.Y),
                                    new PointF(wallCoords.X + 1 - player.Position.X, wallCoords.Y - player.Position.Y),
                                    new PointF(wallCoords.X + 1 - player.Position.X, wallCoords.Y + 1 - player.Position.Y),
                                    new PointF(wallCoords.X - player.Position.X, wallCoords.Y + 1 - player.Position.Y), };
                                // Get distance of the 4 vectors
                                List<double> cornerDistances = new List<double> {
                                    cornerVectors[0].GetLength(),
                                    cornerVectors[1].GetLength(),
                                    cornerVectors[2].GetLength(),
                                    cornerVectors[3].GetLength(), };
                                double maxDistance = cornerDistances.Max(); // Get maximum value in cornerdistances
                                int removeIndex = cornerDistances.IndexOf(maxDistance); // Get index of maximum value in cornerdistances
                                cornerVectors.RemoveAt(removeIndex); // Remove that index from cornerVectors
                                cornerDistances.RemoveAt(removeIndex); // Remove that index from cornerDistances
                                // Remove another vector
                                maxDistance = cornerDistances.Max(); // Get maximum value in cornerdistances
                                removeIndex = cornerDistances.IndexOf(maxDistance); // Get index of maximum value in cornerdistances
                                cornerVectors.RemoveAt(removeIndex); // Remove that index from cornerVectors
                                cornerDistances.RemoveAt(removeIndex); // Remove that index from cornerDistances
                                // compare angle of current Ray with angle of the remaining corners using dotProduct
                                foreach (PointF vector in cornerVectors)
                                {
                                    double dotProduct = rayPoint_player.GetUnitVector().X * vector.GetUnitVector().X + rayPoint_player.GetUnitVector().Y * vector.GetUnitVector().Y;
                                    //Debug.WriteLine(dotProduct);
                                    if (dotProduct > 0.99999 && dotProduct < 1.00001)
                                        hitCorner = true;
                                }

                                if (!hitCorner)
                                {
                                    // Fill this column with wallPiece
                                    char wallChar = wallPieces[(int)(distance / (drawDistance / wallPieces.Length))];
                                    int minWallHeight = 10;
                                    int maxWallHeight = 30; // Maybe sacrifice symmetry to increase distance resolution
                                    int wallHeight = (maxWallHeight - (int)(distance / (drawDistance / (maxWallHeight - minWallHeight))));
                                    for (int row = consoleBufferHeight / 2 - wallHeight / 2; row < consoleBufferHeight / 2 + wallHeight / 2; row++)
                                    {
                                        outputBuilder[row * consoleBufferWidth + column] = wallChar;
                                        //colorBuffer[row * consoleBufferWidth + column] = 
                                    }
                                }

                                //// make ground
                                //for (int row = 24; row < consoleBufferHeight; row++)
                                //{
                                //    char groundChar = groundPieces[(int)(distance / (maximumDistance / 3))];
                                //    outputBuilder[row * consoleBufferWidth + column] = groundChar;
                                //}

                                break; // Next ray
                            }
                        }
                    }
                }

                // Add HUD
                if (HUD.IsActive)
                {
                    string[] insertion = new string[] {
                        "X: " + player.Position.X + " Y: " + player.Position.Y,
                        "a: " + player.fAngle, 
                    };
                    for (int s = 0; s < insertion.Length; s++)
                    {
                        outputBuilder.Remove(s * consoleBufferWidth, insertion[s].Length);
                        outputBuilder.Insert(s * consoleBufferWidth, insertion[s]);
                    }
                }

                // Print
                outputBuilder.Remove(consoleBufferWidth * consoleBufferHeight - 1, 1); // Remove last character to prevent scrolling
                Console.SetCursorPosition(0,0);
                Console.Write(outputBuilder.ToString());

                frameCount++; // Will max out at some point
            }
        }
    }
}
