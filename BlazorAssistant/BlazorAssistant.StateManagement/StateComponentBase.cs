using Microsoft.AspNetCore.Components;

namespace BlazorAssistant.StateManagement
{
    /// <summary>
    /// Base class for Razor components using <see cref="IStateManager"/>.
    /// </summary>
    public abstract class StateComponentBase : ComponentBase, IDisposable
    {
        /// <summary>
        /// The State containing all stored stateful properties.
        /// The manager is the one sending events whenever the state changes.
        /// </summary>
        [Inject]
        protected IStateManager State { get; set; } = default!;

        /// <summary>
        /// The collection holding the active subscriptions for the component.
        /// </summary>
        private readonly List<Action<ComponentChangedEventArgs>> _subscribers = new();

        /// <summary>
        /// The collection holding the active value subscriptions for the component.
        /// </summary>
        private readonly Dictionary<string, Action<ComponentChangedEventArgs>> _valueSubscribers = new();

        /// <summary>
        /// Adds a new subscription to the <see cref="IStateManager.StateChanged" /> event.
        /// Used when custom handling should be performed on the <see cref="ComponentChangedEventArgs"/>.
        /// </summary>
        /// <param name="subscriber">The action to be taken whenever the state changes.</param>
        /// <returns>True if the subscriber was added. False if it was added from before or not added at all.</returns>
        protected bool AddStateChangeSubscriber(Action<ComponentChangedEventArgs> subscriber)
        {
            if (!_subscribers.Contains(subscriber))
            {
                State.StateChanged += subscriber;
                _subscribers.Add(subscriber);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a new subscription to the <see cref="IStateManager.StateChanged" /> event.
        /// Used when action should only be taken on a specific property changes and the source does not matter.
        /// </summary>
        /// <typeparam name="T">The expected type of the changed value in state.</typeparam>
        /// <param name="propertyName">The name of the property add a subscription for.</param>
        /// <param name="valueSubscriber">The action to be performed with the updated value.</param>
        /// <returns>True if the subscriber was added. False if it was added from before or not added at all.</returns>
        protected bool AddStateChangeSubscriber<T>(string propertyName, Action<T> valueSubscriber) 
            where T : notnull
        {
            if (!_valueSubscribers.ContainsKey(propertyName))
            {
                Action<ComponentChangedEventArgs> subscriber = ConstructValueSubscriber(valueSubscriber, new(this, propertyName), false);
                State.StateChanged += subscriber;
                _valueSubscribers.Add(propertyName, subscriber);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a new subscription to the <see cref="IStateManager.StateChanged" /> event.
        /// Used when action should only be taken on a specific property changes and a specific source is expected.
        /// </summary>
        /// <typeparam name="T">The expected type of the changed value in state.</typeparam>
        /// <param name="valueSubscriber">The action to be performed with the updated value.</param>
        /// <param name="args">The expected arguments from the <see cref="IStateManager.StateChanged"/> event.</param>
        /// <param name="onlyInvokeOnSourceTrigger">Flag if the action invoke should be performed only when the sources match.</param>
        /// <returns>True if the subscriber was added. False if it was added from before or not added at all.</returns>
        protected bool AddStateChangeSubscriber<T>(
            Action<T> valueSubscriber, 
            ComponentChangedEventArgs args,
            bool onlyInvokeOnSourceTrigger = true) where T : notnull
        {
            if (!_valueSubscribers.ContainsKey(args.PropertyName))
            {
                Action<ComponentChangedEventArgs> subscriber = ConstructValueSubscriber(valueSubscriber, args, onlyInvokeOnSourceTrigger);
                State.StateChanged += subscriber;
                _valueSubscribers.Add(args.PropertyName, subscriber);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a subscriber if it exists.
        /// </summary>
        /// <param name="subscriber">The subscription to be removed.</param>
        /// <returns>True if the subscriber was found and removed. Else false.</returns>
        protected bool RemoveStateChangeSubscriber(Action<ComponentChangedEventArgs> subscriber)
        {
            if (_subscribers.Remove(subscriber))
            {
                State.StateChanged -= subscriber;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a subscriber if it exists by its <paramref name="propertyName"/>.
        /// Note that this is only possible when a subscriptions is set up providing a property name.
        /// </summary>
        /// <param name="propertyName">The name of the property subscriber to be removed.</param>
        /// <returns>True if the subscriber was found and removed. Else false.</returns>
        protected bool RemoveStateChangeSubscriber(string propertyName)
        {
            if (_valueSubscribers.TryGetValue(propertyName, out Action<ComponentChangedEventArgs>? subscriber)
                && _valueSubscribers.Remove(propertyName))
            {
                State.StateChanged -= subscriber;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all subscriptions, including value subscriptions, for the component.
        /// </summary>
        protected void ClearStateChangeSubscriptions()
        {
            foreach (var subscriber in _subscribers)
            {
                State.StateChanged -= subscriber;
            }

            foreach (var (_, subscriber) in _valueSubscribers)
            {
                State.StateChanged -= subscriber;
            }

            _subscribers.Clear();
            _valueSubscribers.Clear();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Clears all subscriptions on the <see cref="IStateManager.StateChanged"/> event whenever the component disposes.
        /// </remarks>
        public virtual void Dispose()
        {
            ClearStateChangeSubscriptions();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Constructs a value subscriber based on the provided <paramref name="args"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the changed value in state.</typeparam>
        /// <param name="valueSubscriber">The action to be performed with the updated value.</param>
        /// <param name="args">The expected arguments from the <see cref="IStateManager.StateChanged"/> event.</param>
        /// <param name="onlyInvokeOnSourceTrigger">Flag if the action invoke should be performed only when the sources match.</param>
        /// <returns>The constructed value subscriber.</returns>
        private Action<ComponentChangedEventArgs> ConstructValueSubscriber<T>(
            Action<T> valueSubscriber, 
            ComponentChangedEventArgs args, 
            bool onlyInvokeOnSourceTrigger)
            where T : notnull
            => (ComponentChangedEventArgs e) =>
            {
                if (e.PropertyName != args.PropertyName)
                    return;

                if (onlyInvokeOnSourceTrigger && e.Source != args.Source)
                    return;

                valueSubscriber.Invoke(State.Get<T>(args.PropertyName));
            };
    }
}
