using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Priority_Queue;

namespace AdventOfCode2021
{
    public enum Occupant
    {
        A1,
        A2,
        A3,
        A4,
        B1,
        B2,
        B3,
        B4,
        C1,
        C2,
        C3,
        C4,
        D1,
        D2,
        D3,
        D4
    }

    public class GamePath
    {
        public GameSpot Destination { get; }
        private readonly Func<Occupant, int> costFunction;
        private readonly ImmutableList<Func<GameState, Occupant, bool>> predicates;

        public GamePath(GameSpot destination, Func<Occupant, int> costFunction,
            List<Func<GameState, Occupant, bool>> predicates)
        {
            this.Destination = destination;
            this.costFunction = costFunction;
            this.predicates = ImmutableList.Create(predicates.ToArray());
        }
        
        public bool CanTravel(GameState state, Occupant occupant)
        {
            return predicates.All(p => p(state, occupant));
        }

        public int GetCost(Occupant occupant)
        {
            return costFunction(occupant);
        }
        
    }
    
    public class GameSpot
    {
        public int Index { get; }
        private ImmutableList<GamePath> paths;

        private GameSpot(int index)
        {
            this.Index = index;
        }
        
        // Junk accessor for test cheating purposes
        public ImmutableList<GamePath> Paths
        {
            get { return paths; }
        }
        
        public List<GameState> Explore(GameState currentState, Occupant mover)
        {
            throw new NotImplementedException("TODO");
        }

        public class Builder
        {
            private GameSpot spot;
            private List<GamePath> paths = new();

            public Builder(int index)
            {
                spot = new GameSpot(index);
            }

            public void AddPath(Builder destination, Func<Occupant, int> costFunction,
                List<Func<GameState, Occupant, bool>> predicates)
            {
                paths.Add(new GamePath(destination.spot, costFunction, predicates));
            }

            public GameSpot Build()
            {
                spot.paths = ImmutableList.Create(paths.ToArray());
                GameSpot result = spot;
                // Poison builder so it can't be used anymore
                spot = null;
                paths = null;
                return result;
            }
        }
        
    }
    
    public class GameMap
    {
        public ImmutableList<GameSpot> spots;

        public GameMap()
        {
            spots = ImmutableList.Create(BuildMap().ToArray());
        }

        private static IEnumerable<int> RoomIndices()
        {
            return Enumerable.Range(11, 16);
        }

        private static IEnumerable<int> HallwayIndices()
        {
            return Enumerable.Range(0, 11);
        }

        private static int GetRoomNumber(int roomIndex)
        {
            return (roomIndex - 11) / 4;
        }
        
        private static int GetOutsideHallway(int roomIndex)
        {
            var roomNumber = GetRoomNumber(roomIndex);
            return 2 + 2 * roomNumber;
        }

        private static Func<Occupant, int> MakeCostFunction(int numberOfRooms)
        {
            return (Occupant occupant) => GameMap.MoveCost(occupant) * numberOfRooms;
        }

        private static Func<GameState, Occupant, bool> MakeIsHallwayClear(int startingHallway, int endingHallway, bool includeStart)
        {
            return (GameState state, Occupant occupant) =>
            {
                return state.IsHallwayClear(startingHallway, endingHallway, includeStart);
            };
        }

        private static Func<GameState, Occupant, bool> MakeCanReachRoomDepth(int roomIndex)
        {
            var roomNumber = GetRoomNumber(roomIndex);
            var start = 11 + roomNumber * 4;
            var end = roomIndex;
            return (state, occupant) =>
            {
                return state.CanReachRoomDepth(start, end);
            };
        }
        
        private static Func<GameState, Occupant, bool> MakeIsOccupantsRoom(int roomNumber)
        {
            return (state, occupant) =>
            {
                return ((int)occupant / 4) == roomNumber;
            };
        }

        private static Func<GameState, Occupant, bool> MakeIsRoomEnterable(int roomNumber)
        {
            int roomStart = 11 + 4 * roomNumber;
            int roomEnd = roomStart + 3;
            return (state, occupant) =>
            {
                return state.IsRoomAppropriate(roomNumber, roomStart, roomEnd);
            };
        }

        private static Func<GameState, Occupant, bool> MakeIsDeepestOpenSpot(int index, int roomNumber)
        {
            int roomEnd = 11 + 4 * roomNumber + 3;
            if (roomEnd == index)
            {
                return null;
            }
            return (state, occupant) =>
            {
                return state.IsRoomFull(index + 1, roomEnd);
            };
        }
        
        private static int GetRoomDepth(int index)
        {
            int roomNumber = GetRoomNumber(index);
            int roomStart = 11 + 4 * roomNumber;
            return index - roomStart;
        }

        private static bool IsRoomEntrance(int index)
        {
            return index > 0 && index < 10 && index % 2 == 0;
        }
        
        private static void AddRoomToHallwayPaths(List<GameSpot.Builder> spots, int roomIndex)
        {
            var outsideHallway = GetOutsideHallway(roomIndex);
            foreach (var hallwaySpot in HallwayIndices())
            {
                if (IsRoomEntrance(hallwaySpot))
                {
                    // Cannot move to the hallway outside your room
                    continue;
                }

                // # of squares beyond the first we need to move
                int delta = Math.Abs(hallwaySpot - outsideHallway);
                int roomDepth = GetRoomDepth(roomIndex);
                delta += roomDepth + 1;
                var costFunction = MakeCostFunction(delta);
                var isHallwayClear = MakeIsHallwayClear(outsideHallway, hallwaySpot, true);
                List<Func<GameState, Occupant, bool>> predicates = new() { isHallwayClear };
                if (roomDepth > 0)
                {
                    predicates.Add(MakeCanReachRoomDepth(roomIndex - 1));
                }
                spots[roomIndex].AddPath(spots[hallwaySpot], costFunction, predicates);
            }
        }

        private static void AddHallwayToRoomPaths(List<GameSpot.Builder> spots, int hallwayIndex)
        {
            foreach (var roomNumber in new int[] {0, 1, 2, 3})
            {
                foreach (var depth in new int[] {0, 1, 2, 3})
                {
                    var destinationIndex = 11 + 4 * roomNumber + depth;
                    var outerHallway = GetOutsideHallway(destinationIndex);
                    var distanceToSpot = Math.Abs(outerHallway - hallwayIndex) + depth + 1;
                    var costFunction = MakeCostFunction(distanceToSpot);
                    var predicates = new List<Func<GameState, Occupant, bool>>();
                    predicates.Add(MakeIsHallwayClear(hallwayIndex, outerHallway, false));
                    predicates.Add(MakeIsOccupantsRoom(roomNumber));
                    predicates.Add(MakeCanReachRoomDepth(destinationIndex));
                    predicates.Add(MakeIsRoomEnterable(roomNumber));
                    var isDeepestOpenSlot = MakeIsDeepestOpenSpot(destinationIndex, roomNumber);
                    if (isDeepestOpenSlot != null)
                    {
                        predicates.Add(MakeIsDeepestOpenSpot(destinationIndex, roomNumber));
                    }
                    spots[hallwayIndex].AddPath(spots[destinationIndex], costFunction, predicates);
                }
            }
        }
        
        private static List<GameSpot> BuildMap()
        {
            List<GameSpot.Builder> spots = new();
            // Hallway
            for (int i = 0; i < 27; i++)
            {
                spots.Add(new GameSpot.Builder(i));
            }
            // Add room -> hallway paths
            foreach (var index in RoomIndices())
            {
                AddRoomToHallwayPaths(spots, index);
            }

            foreach (var index in HallwayIndices())
            {
                AddHallwayToRoomPaths(spots, index);
            }

            return spots.Select(spot => spot.Build()).ToList();
        }
        
        public static int MoveCost(Occupant occ)
        {
            return (int) Math.Pow(10, ((int) occ) / 4);
        }
    }

    public class GameState
    {
        private GameMap map;
        private sbyte[] occupants;
        public int CurrentCost { private set; get; }
        public List<GameState> CurrentPath { private set; get; }
        public ValueTuple<long, long> Key { get; }

        private GameState(GameMap map, sbyte[] occupants, int cost)
        {
            this.map = map;
            this.occupants = occupants;
            this.CurrentCost = cost;
            this.CurrentPath = new List<GameState>();
            Key = ToKey();
        }

        private static char[] OCCUPANT_CHARS = new char[] {'A', 'B', 'C', 'D'};
        
        private static char GetSpotChar(Dictionary<int, Occupant> spotsToOccupant, int spot, char def)
        {
            if (spotsToOccupant.TryGetValue(spot, out var occupant))
            {
                int occupantType = ((int)occupant) / 4;
                return OCCUPANT_CHARS[occupantType];
            }
            else
            {
                return def;
            }
        }

        private Dictionary<int, Occupant> GetSpotsToOccupant()
        {
            var result = new Dictionary<int, Occupant>();
            for (int i = 0; i < occupants.Length; i++)
            {
                result[occupants[i]] = (Occupant) i;
            }

            return result;
        }
        
        public override string ToString()
        {
            var spotsToOccupant = GetSpotsToOccupant();
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < 11; i++)
            {
                stringBuilder.Append(GetSpotChar(spotsToOccupant, i, '.'));
            }

            stringBuilder.Append(' ');
            stringBuilder.Append(CurrentCost);
            stringBuilder.AppendLine();
            
            int spot = 11;
            for (int i = 0; i < 4; i++)
            {
                stringBuilder.Append('#');
                for (int j = 0; j < 4; j++)
                {
                    stringBuilder.Append('#');
                    stringBuilder.Append(GetSpotChar(spotsToOccupant, spot, '.'));
                    spot += 4;
                }

                stringBuilder.Append('#');
                stringBuilder.Append('#');
                stringBuilder.AppendLine();

                spot = spot - 16 + 1;
            }
            return stringBuilder.ToString();
            
        }

        private static GameState CloneWithMove(GameState start, int srcRoom, Occupant mover, GamePath path)
        {
            var newCost = start.CurrentCost + path.GetCost(mover);
            var newOccupants = (sbyte[]) start.occupants.Clone();
            newOccupants[(int)mover] = (sbyte) path.Destination.Index;
            var result = new GameState(start.map, newOccupants, newCost);
            result.CurrentPath.AddRange(start.CurrentPath);
            result.CurrentPath.Add(result);
            return result;
        }
        
        public IEnumerable<GameState> Explore()
        {
            for (int i = 0; i < occupants.Length; i++)
            {
                Occupant mover = (Occupant) i;
                GameSpot moverSpot = map.spots[occupants[i]];
                foreach (var path in moverSpot.Paths)
                {
                    if (path.CanTravel(this, mover))
                    {
                        yield return GameState.CloneWithMove(this, occupants[i], mover, path);
                    }
                }
            }
        }

        public void UpdateCost(int newCost, List<GameState> newPath)
        {
            CurrentCost = newCost;
            CurrentPath = newPath;
        }
        
        public ValueTuple<long, long> ToKey()
        {
            sbyte[] codes = new sbyte[27];
            for (int i = 0; i < 27; i++)
            {
                codes[i] = -1;
            }

            for (int i = 0; i < occupants.Length; i++)
            {
                codes[occupants[i]] = (sbyte) (((byte) i) / 4);
            }
            long part1 = 0;
            long mult = 1;
            for (int i = 0; i < codes.Length / 2; i++)
            {
                part1 += mult * codes[i];
                mult *= 8;
            }

            mult = 1;
            long part2 = 0;
            for (int i = codes.Length / 2; i < codes.Length; i++)
            {
                part2 += mult * codes[i];
                mult *= 8;
            }

            return (part1, part2);
        }

        public bool IsHallwayClear(int startHallway, int endHallway, bool includeStart)
        {
            int smallest, largest;
            if (startHallway < endHallway)
            {
                smallest = startHallway;
                if (!includeStart)
                {
                    smallest++;
                }

                largest = endHallway;
            }
            else
            {
                smallest = endHallway;
                largest = startHallway;
                if (!includeStart)
                {
                    largest--;
                }
            }
            foreach (var occupiedIndex in occupants)
            {
                if ( occupiedIndex >= smallest && occupiedIndex <= largest)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanReachRoomDepth(int roomStart, int roomEnd)
        {
            foreach (var occupiedIndex in occupants)
            {
                if (occupiedIndex >= roomStart && occupiedIndex <= roomEnd)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsRoomAppropriate(int roomNumber, int roomStart, int roomEnd)
        {
            for (int occupentIndex = 0; occupentIndex < occupants.Length; occupentIndex++)
            {
                int occupiedIndex = occupants[occupentIndex];
                if (occupiedIndex >= roomStart && occupiedIndex <= roomEnd)
                {
                    if (occupentIndex / 4 != roomNumber)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsRoomFull(int roomStart, int roomEnd)
        {
            for (int i = roomStart; i < roomEnd; i++)
            {
                bool found = false;
                foreach (var occupiedIndex in occupants)
                {
                    if (occupiedIndex == i)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsRoomClear(int roomIndex)
        {
            foreach (var occupiedIndex in occupants)
            {
                if (roomIndex == occupiedIndex)
                {
                    return false;
                }
            }
            return true;
        }

        public Occupant GetOccupant(int roomIndex)
        {
            for (int i = 0; i < occupants.Length; i++)
            {
                if (occupants[i] == roomIndex)
                {
                    return (Occupant) i;
                }
            }

            throw new Exception("Expected to find someone in the given room index but didn't");
        }
        
        private int GetDesiredRoomNumber(int indexInOccupants)
        {
            return indexInOccupants / 4;
        }
        
        public bool IsTerminal()
        {
            for (int i = 0; i < occupants.Length; i++)
            {
                int spot = occupants[i];
                if (spot < 11)
                {
                    return false;
                }

                int currentRoomNumber = (spot - 11) / 4;
                int desiredRoomNumber = GetDesiredRoomNumber(i);
                if (currentRoomNumber != desiredRoomNumber)
                {
                    return false;
                }
            }

            return true;
        }

        private static sbyte[] InitialOccupants()
        {
            sbyte[] occupants = new sbyte[16];
            for (int i = 0; i < occupants.Length; i++)
            {
                occupants[i] = -1;
            }

            return occupants;
        }

        public static GameState FromShortInput(List<string> input)
        {
            GameMap map = new GameMap();
            sbyte[] occupants = InitialOccupants();
            int i = 0;
            while (i < 27)
            {
                foreach (var str in input)
                {
                    foreach (var c in str)
                    {
                        if (c == '.')
                        {
                            i++;
                        } else if (c != '#')
                        {
                            int type = GetOccupantTypeFromChar(c);
                            SetOccupant(type, i, occupants);
                            i++;
                        }
                    }
                }
            }

            GameState result = new GameState(map, occupants, 0);
            result.CurrentPath.Add(result);
            return result;
        }

        private static int GetOccupantTypeFromChar(char c)
        {
            switch (c)
            {
                case 'A':
                    return 0;
                case 'B':
                    return 1;
                case 'C':
                    return 2;
                case 'D':
                    return 3;
                default:
                    throw new Exception("Logic error");
            }
        }

        private static void SetOccupant(int type, int index, sbyte[] occupants)
        {
            int occupant_idx = type * 4;
            while (occupants[occupant_idx] != -1)
            {
                occupant_idx++;
            }

            occupants[occupant_idx] = (sbyte) index;
        }
    }

    public class Algorithm
    {
        private readonly SimplePriorityQueue<GameState, int> unvisitedStates = new();
        private readonly Dictionary<ValueTuple<long, long>, GameState> unvisitedStatesMap = new();
        private readonly Dictionary<ValueTuple<long, long>, GameState> visitedStatesMap = new();

        private readonly HashSet<ValueTuple<long, long>> testPath = new HashSet<(long, long)>();
        private int numTestPathsEncountered = 0;
        
        private void Visit(GameState state)
        {
            visitedStatesMap[state.Key] = state;
            unvisitedStatesMap.Remove(state.Key);
            foreach (var nextState in state.Explore())
            {
                if (visitedStatesMap.ContainsKey(nextState.Key))
                {
                    // We've already visited it, we know it won't get any cheaper, so skip on
                }
                else if (unvisitedStatesMap.ContainsKey(nextState.Key))
                {
                    GameState prevVersion = unvisitedStatesMap[nextState.Key];
                    if (nextState.CurrentCost < prevVersion.CurrentCost)
                    {
                        prevVersion.UpdateCost(nextState.CurrentCost, nextState.CurrentPath);
                        unvisitedStates.UpdatePriority(prevVersion, prevVersion.CurrentCost);
                    }
                }
                else
                {
                    if (testPath.Contains(nextState.Key))
                    {
                        numTestPathsEncountered++;
                        Console.WriteLine($"Encountered expected path {numTestPathsEncountered}");
                        Console.WriteLine(nextState);
                    }
                    unvisitedStatesMap[nextState.Key] = nextState;
                    unvisitedStates.Enqueue(nextState, nextState.CurrentCost);
                }
            }
        }
        
        public GameState Run(GameState initialState, int bailAfter = -1, List<GameState> testPath = null)
        {
            if (testPath != null)
            {
                foreach (var state in testPath)
                {
                    this.testPath.Add(state.Key);
                }
            }
            GameState currentState = initialState;
            int numSteps = 0;
            while (!currentState.IsTerminal())
            {
                if (bailAfter == numSteps)
                {
                    Console.WriteLine(currentState);
                    Console.WriteLine();
                    foreach (var possibleState in currentState.Explore())
                    {
                        Console.WriteLine(possibleState);
                        Console.WriteLine();
                    }

                    return null;
                }

                if (numSteps % 10000 == 0)
                {
                    Console.WriteLine($"{unvisitedStates.Count} states.  Current cost: {currentState.CurrentCost}");
                }

                numSteps++;
                Visit(currentState);
                currentState = unvisitedStates.Dequeue();
            }

            return currentState;
        }
    }
    
    public class Day23
    {
        
    }
}