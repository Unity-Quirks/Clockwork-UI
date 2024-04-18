using System;
using UnityEngine.Events;

namespace Quirks.UI
{
    /// <summary>
    /// UnityEvent callback for when a toggle is togggled
    /// </summary>
    [Serializable] public class ToggleEvent : UnityEvent<bool> { }

}