using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StatelessTest
{
    /// <summary>
    /// Enum containing the possible states that the house could be in.
    /// A state can be a substate of another state.
    /// </summary>
    [Serializable]
    //[JsonConverter(typeof(StringEnumConverter))]
    public enum State
    {
        UnOccupied,
        Occupied,
        Asleep,
        ChildOccupied
    }
}
