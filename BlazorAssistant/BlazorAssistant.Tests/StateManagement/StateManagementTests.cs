using BlazorAssistant.StateManagement;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using NSubstitute;

namespace BlazorAssistant.Tests.StateManagement
{
    internal class StateManagerTestWrapper
    {
        internal IStateManager State { get; init; }
        internal bool EventInvoked { get; private set; }

        public StateManagerTestWrapper()
        {
            State = new StateManager();
            State.StateChanged += (_) => EventInvoked = true;
        }
    }

    public class StateManagementTests
    {
        private const string StateKey = "Test";
        private const string StateValue = "Value";

        [Fact]
        public void Initialize_WithoutTriggerStateChangeFlagSet_OnlySetsValue()
        {
            StateManagerTestWrapper state = new();

            state.State.Initialize(StateKey, StateValue);

            state.State.Get<string>(StateKey).Should().Be(StateValue);
            state.EventInvoked.Should().BeFalse();
        }

        [Fact]
        public void Initialize_WithTriggerStateChangeFlagSet_SetsValueAndFiresEvent()
        {
            StateManagerTestWrapper state = new();

            state.State.Initialize(StateKey, StateValue, true, Substitute.For<ComponentBase>());

            state.State.Get<string>(StateKey).Should().Be(StateValue);
            state.EventInvoked.Should().BeTrue();
        }

        [Fact]
        public void Initialize_PropertyIsAlreadyRegisterd_ShouldThrowException()
        {
            StateManagerTestWrapper state = new();
            state.State.Initialize(StateKey, StateValue);
            Assert.Throws<ArgumentException>(() => state.State.Initialize(StateKey, "Value Again"));
        }

        [Fact]
        public void Initialize_WithTriggerStateChangeFlagSet_WithNoSourceSet_ShouldThrowException()
        {
            StateManagerTestWrapper state = new();
            Assert.Throws<ArgumentException>(() => state.State.Initialize(StateKey, StateValue, true));
        }

        [Fact]
        public void Initialize_ShouldBePossibleToSetPropertiesWithDifferentCasing()
        {
            StateManagerTestWrapper state = new();
            state.State.Initialize("Test", 1);
            state.State.Initialize("test", 2);

            state.State.Get<int>("Test").Should().Be(1);
            state.State.Get<int>("test").Should().Be(2);
        }

        [Fact]
        public void Get_WhenKeyDoesNotExists_ThrowsException()
        {
            StateManagerTestWrapper state = new();
            Assert.Throws<KeyNotFoundException>(() => state.State.Get<string>(StateKey));
        }

        [Fact]
        public void Get_WhenGivenTypeDoesNotMatchStoredType_ThrowsException()
        {
            StateManagerTestWrapper state = new();
            state.State.Initialize(StateKey, StateValue);
            Assert.Throws<ArgumentException>(() => state.State.Get<int>(StateKey));
        }

        [Fact]
        public void Set_WhenValueIsUpdated_EventIsInvoked()
        {
            StateManagerTestWrapper state = new();
            state.State.Initialize(StateKey, StateValue);
            state.State.Set(Substitute.For<ComponentBase>(), StateKey, "New Value");

            state.EventInvoked.Should().BeTrue();
        }

        [Fact]
        public void Set_WhenTypeOfValueDoesNotMatch_ThrowsException()
        {
            StateManagerTestWrapper state = new();
            state.State.Initialize(StateKey, StateValue);
            Assert.Throws<ArgumentException>(() => state.State.Set(Substitute.For<ComponentBase>(), StateKey, 1));
        }

        [Fact]
        public void Set_WhenGivenKeyDoesNotExists_ThrowsException()
        {
            StateManagerTestWrapper state = new();
            Assert.Throws<KeyNotFoundException>(() => state.State.Set(Substitute.For<ComponentBase>(), StateKey, StateValue));
        }

        [Fact]
        public void Remove_WhenKeyExists_RemovesTheState()
        {
            StateManagerTestWrapper state = new();
            state.State.Initialize(StateKey, StateValue);
            state.State.Remove(StateKey);
            Assert.Throws<KeyNotFoundException>(() => state.State.Get<string>(StateKey));
        }

        [Fact]
        public void Remove_WhenKeyDoesNotExists_ThrowsException()
        {
            StateManagerTestWrapper state = new();
            Assert.Throws<KeyNotFoundException>(() => state.State.Remove(StateKey));
        }

        [Fact]
        public void Clear_WhenCalled_ClearsTheState()
        {
            StateManagerTestWrapper state = new();
            state.State.Initialize(StateKey, StateValue);
            state.State.Initialize("Key", 1);
            state.State.Clear();

            Assert.Throws<KeyNotFoundException>(() => state.State.Get<string>(StateKey));
            Assert.Throws<KeyNotFoundException>(() => state.State.Get<int>("Key"));
        }
    }
}
