using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace BlazorAssistant.StateManagement
{
    public sealed record ComponentChangedEventArgs(ComponentBase Source, string PropertyName);

    public interface IStateManager
    {
        /// <summary>
        /// Event indicating that something in the state has change.
        /// </summary>
        event Action<ComponentChangedEventArgs> StateChanged;

        /// <summary>
        /// Initalizes a state for a property.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The initial value of the property.</param>
        /// <param name="triggerStateChanged">Flag if a <see cref="StateChanged"/> event should be triggered upon initialization. Default to false.</param>
        /// <param name="source">The source initializing the state. Default to false. Needed if <paramref name="triggerStateChanged"/> is set to true.</param>
        void Initialize<T>(string propertyName, T value, bool triggerStateChanged = false, ComponentBase? source = null) where T : notnull;

        /// <summary>
        /// Get the current state value of a property.
        /// </summary>
        /// <typeparam name="T">The expected type of the property.</typeparam>
        /// <param name="propertyName">The name of stored property in state.</param>
        /// <returns>The current state value of <paramref name="propertyName"/>.</returns>
        T Get<T>(string propertyName) where T : notnull;

        /// <summary>
        /// Sets a new value for an already initialized state.
        /// Fires <see cref="StateChanged"/> upon invoke.
        /// </summary>
        /// <typeparam name="T">The type of the property. Needs to be the same as the initalized type.</typeparam>
        /// <param name="source">The component performing the change.</param>
        /// <param name="propertyName">The name of the property being changed.</param>
        /// <param name="value">The new state value of the property.</param>
        void Set<T>(ComponentBase source, string propertyName, T value) where T : notnull;

        /// <summary>
        /// Removes a property from the state.
        /// </summary>
        /// <param name="propertyName">The name of the property to remove.</param>
        void Remove(string propertyName);

        /// <summary>
        /// Clears the entire state.
        /// </summary>
        void Clear();
    }

    public sealed class StateManager : IStateManager
    {
        private readonly ConcurrentDictionary<string, object> _state = new();
        public event Action<ComponentChangedEventArgs>? StateChanged;

        /// <inheritdoc />
        public T Get<T>(string propertyName) where T : notnull
        {
            if (_state.TryGetValue(propertyName, out object? storedValue))
            {
                if (storedValue is T value)
                {
                    return value;
                }
                else
                {
                    throw new ArgumentException("Provided type does not match with type in state.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"No state of property {propertyName} has been registered.");
            }
        }

        /// <inheritdoc />
        public void Initialize<T>(string propertyName, T value, bool triggerStateChanged = false, ComponentBase? source = null) where T: notnull
        {
            lock (_state)
            {
                if (!_state.TryAdd(propertyName, value))
                {
                    throw new ArgumentException($"{propertyName} is already registered in state.");
                }

                if (triggerStateChanged)
                {
                    if (source is null)
                        throw new ArgumentException($"A source must be set when triggerStateChanged is set to true");

                    NotifyStateChanged(source, propertyName);
                }
            }
        }

        /// <inheritdoc />
        public void Set<T>(ComponentBase source, string propertyName, T value) where T : notnull
        {
            lock(_state)
            {
                if (_state.ContainsKey(propertyName))
                {
                    if (_state[propertyName] is not T currentValue)
                        throw new ArgumentException("Provided type does not match with type in state.");

                    if (_state.TryUpdate(propertyName, value, currentValue))
                    {
                        NotifyStateChanged(source, propertyName);
                    }
                    else
                    {
                        throw new Exception($"Failed to update state for {propertyName}.");
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"No state with name {propertyName} has been registered.");
                }
            }
        }

        /// <inheritdoc />
        public void Remove(string propertyName)
        {
            lock (_state)
            {
                if (!_state.Remove(propertyName, out _))
                {
                    throw new KeyNotFoundException($"No state with name {propertyName} has been registered.");
                }
            }
        }

        /// <inheritdoc />
        public void Clear() => _state.Clear();

        /// <summary>
        /// Shorthand method to invoke <see cref="StateChanged"/>.
        /// </summary>
        /// <param name="source">The component performing the change.</param>
        /// <param name="propertyName">The name of the property being changed.</param>
        private void NotifyStateChanged(ComponentBase source, string propertyName) => StateChanged?.Invoke(new(source, propertyName));
    }
}
