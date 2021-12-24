using System;
using System.Collections.Generic;

namespace AdventOfCode2021
{
    class Program
    {
        static void TestOne()
        {
            GameMap map = new GameMap();
            foreach (var spot in map.spots)
            {
                Console.WriteLine($"From {spot.Index}");
                foreach (var path in spot.Paths)
                {
                    Console.WriteLine($"  Can move to {path.Destination.Index} for a cost of {path.GetCost(Occupant.A1)}");
                }
            }
        }
        
        static void CheckTerminal(List<string> inp, bool expected)
        {
            GameState state = GameState.FromShortInput(inp);
            bool isTerminal = state.IsTerminal();
            if (isTerminal != expected)
            {
                throw new Exception($"Expected isTerminal to be {expected} but was {isTerminal}");
            }
        }
        
        static void CheckTerminal(List<string> inp)
        {
            CheckTerminal(inp, true);
        }

        static void CheckNotTerminal(List<string> inp)
        {
            CheckTerminal(inp, false);
        }
        
        static void TestIsTerminal()
        {
            CheckTerminal(new List<string>
            {
                "...........",
                "AAAA#BBBB#CCCC#DDDD"
            });
            CheckNotTerminal(new List<string>
            {
                "...........",
                "BBBB#AAAA#CCCC#DDDD"
            });
            CheckNotTerminal(new List<string>
            {
                ".A...B.....",
                "A.AA#B.BB#CCCC#DDDD"
            });
        }
        
        static void Main(string[] args)
        {
            TestIsTerminal();
            // GameState initialState = GameState.FromShortInput(new List<string>
            // {
            //     "...........",
            //     "BDDA#CCBD#BBAC#DACA"
            // });
            GameState initialState = GameState.FromShortInput(new List<string>
            {
                "...........",
                "ADDB#CCBA#BBAD#DACC"
            });
            var possiblePath = new List<GameState>
            {
                GameState.FromShortInput(new List<string>
                {
                    "...........",
                    "BDDA#CCBD#BBAC#DACA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "..........D",
                    "BDDA#CCBD#BBAC#.ACA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "A.........D",
                    "BDDA#CCBD#BBAC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "A........BD",
                    "BDDA#CCBD#.BAC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "A......B.BD",
                    "BDDA#CCBD#..AC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.....B.BD",
                    "BDDA#CCBD#...C#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA...C.B.BD",
                    "BDDA#.CBD#...C#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.....B.BD",
                    "BDDA#.CBD#..CC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA...C.B.BD",
                    "BDDA#..BD#..CC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.....B.BD",
                    "BDDA#..BD#.CCC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA...B.B.BD",
                    "BDDA#...D#.CCC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D.B.B.BD",
                    "BDDA#....#.CCC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D...B.BD",
                    "BDDA#...B#.CCC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D.....BD",
                    "BDDA#..BB#.CCC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D......D",
                    "BDDA#.BBB#.CCC#..CA"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D...C..D",
                    "BDDA#.BBB#.CCC#...A"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D......D",
                    "BDDA#.BBB#CCCC#...A"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D.....AD",
                    "BDDA#.BBB#CCCC#...."
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.......AD",
                    "BDDA#.BBB#CCCC#...D"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.B.....AD",
                    ".DDA#.BBB#CCCC#...D"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.......AD",
                    ".DDA#BBBB#CCCC#...D"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D.....AD",
                    "..DA#BBBB#CCCC#...D"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.......AD",
                    "..DA#BBBB#CCCC#..DD"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "AA.D.....AD",
                    "...A#BBBB#CCCC#..DD"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "A..D.....AD",
                    "..AA#BBBB#CCCC#..DD"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "...D.....AD",
                    ".AAA#BBBB#CCCC#..DD"
                }),
                GameState.FromShortInput(new List<string>
                {
                    ".........AD",
                    ".AAA#BBBB#CCCC#.DDD"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "..........D",
                    "AAAA#BBBB#CCCC#.DDD"
                }),
                GameState.FromShortInput(new List<string>
                {
                    "...........",
                    "AAAA#BBBB#CCCC#DDDD"
                }),
            };
            var node = possiblePath[0];
            int nextIndex = 1;
            while (nextIndex < possiblePath.Count)
            {
                var next = possiblePath[nextIndex];
                bool found = false;
                foreach (var possible in node.Explore())
                {
                    if (possible.Key == next.Key)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Console.WriteLine("Couldn't find possibility");
                    Console.WriteLine();
                    Console.WriteLine(node);
                    Console.WriteLine();
                    Console.WriteLine(next);
                    Console.WriteLine(nextIndex);
                    return;
                }

                node = next;
                nextIndex++;
            }
            Console.WriteLine("Was able to trace solution");
            
            var algorithm = new Algorithm();
            int bailSteps = -1;
            if (args.Length > 0)
            {
                bailSteps = Int32.Parse(args[0]);
            }
            Console.WriteLine($"Bailing after {bailSteps} steps");
            GameState finalState = algorithm.Run(initialState, bailSteps, possiblePath);
            Console.WriteLine(finalState.CurrentCost);
            foreach (var state in finalState.CurrentPath)
            {
                Console.WriteLine(state);
                Console.WriteLine();
            }
        }
    }
}
