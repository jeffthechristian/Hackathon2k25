/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Oculus.Interaction.Throw;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Oculus.Interaction
{
    /// <summary>
    /// The <see cref="Grabbable"/> class enables interaction with and manipulation of the object it's attached to when an <see cref="Interactor{TInteractor,TInteractable}"/> selects that object.
    /// This class implements the <see cref="IGrabbable"/> and <see cref="ITimeConsumer"/> interfaces and inherits from <see cref="PointableElement"/> to handle pointer events effectively.
    /// </summary>
    /// <remarks>
    /// Utilize this class to make any GameObject interactive and responsive to grab-based user interactions. It supports both single-hand and dual-hand interactions, allowing for complex object manipulation.
    /// </remarks>
    public class Grabbable : PointableElement, IGrabbable, ITimeConsumer
    {
        [SerializeField]
        private float _throwVelocityMultiplier = 1.0f;

        [SerializeField]
        private float _throwAngularVelocityMultiplier = 1.0f;

        /// <summary>
        /// A One Grab...Transformer component, which should be attached to the grabbable object. Defaults to One Grab Free Transformer.
        /// If you set the Two Grab Transformer property and still want to use one hand for grabs, you must set this property as well.
        /// </summary>
        [Tooltip("A One Grab...Transformer component, which should be attached to the grabbable object. Defaults to One Grab Free Transformer. If you set the Two Grab Transformer property and still want to use one hand for grabs, you must set this property as well.")]
        [SerializeField, Interface(typeof(ITransformer))]
        [Optional(OptionalAttribute.Flag.AutoGenerated)]
        private UnityEngine.Object _oneGrabTransformer = null;

        /// <summary>
        /// A Two Grab...Transformer component, which should be attached to the grabbable object.
        /// If you set this property but also want to use one hand for grabs, you must set the One Grab Transformer property.
        /// </summary>
        [Tooltip("A Two Grab...Transformer component, which should be attached to the grabbable object. If you set this property but also want to use one hand for grabs, you must set the One Grab Transformer property.")]
        [SerializeField, Interface(typeof(ITransformer))]
        [Optional(OptionalAttribute.Flag.AutoGenerated)]
        private UnityEngine.Object _twoGrabTransformer = null;

        /// <summary>
        /// Takes a target object to transform instead of transforming the object that has the Grabbable component.
        /// The object with the Grabbable component acts as a controller that projects its transforms onto the target object.
        /// </summary>
        [Tooltip("The target transform of the Grabbable. If unassigned, " +
            "the transform of this GameObject will be used.")]
        [SerializeField]
        [Optional(OptionalAttribute.Flag.AutoGenerated)]
        private Transform _targetTransform;

        /// <summary>
        /// The maximum number of grab points. Can be either -1 (unlimited), 1, or 2.
        /// </summary>
        [Tooltip("The maximum number of grab points. Can be either -1 (unlimited), 1, or 2.")]
        [SerializeField, Min(-1)]
        private int _maxGrabPoints = -1;

        [Header("Physics")]
        [SerializeField, Optional(OptionalAttribute.Flag.DontHide)]
        [Tooltip("Use this rigidbody to control its physics properties while grabbing.")]
        private Rigidbody _rigidbody;
        [SerializeField]
        [Tooltip("Locks the referenced rigidbody to a kinematic while selected.")]
        private bool _kinematicWhileSelected = true;
        [SerializeField]
        [Tooltip("Applies throwing velocities to the rigidbody when fully released.")]
        private bool _throwWhenUnselected = true;

        /// <summary>
        /// Gets or sets the maximum number of grab points. This property is crucial for defining how many points can be used to interact with the object.
        /// </summary>
        public int MaxGrabPoints
        {
            get
            {
                return _maxGrabPoints;
            }
            set
            {
                _maxGrabPoints = value;
            }
        }

        /// <summary>
        /// Provides access to the transform that should be manipulated. This can be the transform of the GameObject this component is attached to or a different specified transform.
        /// </summary>
        public Transform Transform => _targetTransform;

        /// <summary>
        /// Lists the current grab points as poses. These are used to calculate transformations based on user interactions.
        /// </summary>
        public List<Pose> GrabPoints => _selectingPoints;

        private Func<float> _timeProvider = () => Time.time;

        /// <summary>
        /// Sets a custom time provider function that returns the current time in seconds. This is essential for synchronizing time-dependent behaviors within the <see cref="Grabbable"/> object.
        /// </summary>
        /// <param name="timeProvider">A function delegate that returns the current time in seconds.</param>
        public void SetTimeProvider(Func<float> timeProvider)
        {
            _timeProvider = timeProvider;
            if (_throw != null)
            {
                _throw.SetTimeProvider(timeProvider);
            }
        }

        private ITransformer _activeTransformer = null;
        private ITransformer OneGrabTransformer;
        private ITransformer TwoGrabTransformer;

        private ThrowWhenUnselected _throw;

        private bool _isKinematicLocked = false;

        #region Editor
        protected virtual void Reset()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            OneGrabTransformer = _oneGrabTransformer as ITransformer;
            TwoGrabTransformer = _twoGrabTransformer as ITransformer;
        }

        protected override void Start()
        {
            this.BeginStart(ref _started, () => base.Start());

            if (_targetTransform == null)
            {
                _targetTransform = transform;
            }

            if (_oneGrabTransformer != null)
            {
                this.AssertField(OneGrabTransformer, nameof(OneGrabTransformer));
                OneGrabTransformer.Initialize(this);
            }

            if (_twoGrabTransformer != null)
            {
                this.AssertField(TwoGrabTransformer, nameof(TwoGrabTransformer));
                TwoGrabTransformer.Initialize(this);
            }

            // Create missing defaults
            if (OneGrabTransformer == null && TwoGrabTransformer == null)
            {
                ITransformer transformer = GenerateTransformer();
                transformer.Initialize(this);
            }

            if (_rigidbody != null && _throwWhenUnselected)
            {
                _throw = new ThrowWhenUnselected(_rigidbody, this); // Pass Grabbable instance
                _throw.SetTimeProvider(this._timeProvider);
            }

            this.EndStart(ref _started);
        }

        protected override void OnDisable()
        {
            if (_started)
            {
                EndTransform();
            }

            base.OnDisable();
        }

        protected virtual void OnDestroy()
        {
            if (_throw != null)
            {
                _throw.Dispose();
                _throw = null;
            }
        }

        private ITransformer GenerateTransformer()
        {
            ITransformer transformer = gameObject.AddComponent<GrabFreeTransformer>();
            this.InjectOptionalOneGrabTransformer(transformer);
            this.InjectOptionalTwoGrabTransformer(transformer);
            return transformer;
        }

        public override void ProcessPointerEvent(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Select:
                    EndTransform();
                    break;
                case PointerEventType.Unselect:
                    ForceMove(evt);
                    EndTransform();
                    break;
                case PointerEventType.Cancel:
                    EndTransform();
                    break;
            }

            base.ProcessPointerEvent(evt);

            switch (evt.Type)
            {
                case PointerEventType.Select:
                    BeginTransform();
                    break;
                case PointerEventType.Unselect:
                    BeginTransform();
                    break;
                case PointerEventType.Move:
                    UpdateTransform();
                    break;
            }
        }

        protected override void PointableElementUpdated(PointerEvent evt)
        {
            UpdateKinematicLock(SelectingPointsCount > 0);

            base.PointableElementUpdated(evt);
        }

        private void UpdateKinematicLock(bool isGrabbing)
        {
            if (_rigidbody == null
                || !_kinematicWhileSelected)
            {
                return;
            }

            if (!_isKinematicLocked && isGrabbing)
            {
                _isKinematicLocked = true;
                _rigidbody.LockKinematic();
            }
            else if (_isKinematicLocked && !isGrabbing)
            {
                _isKinematicLocked = false;
                _rigidbody.UnlockKinematic();
            }
        }

        private void ForceMove(PointerEvent releaseEvent)
        {
            PointerEvent moveEvent = new PointerEvent(releaseEvent.Identifier,
                PointerEventType.Move, releaseEvent.Pose, releaseEvent.Data);
            ProcessPointerEvent(moveEvent);
        }

        // Whenever we change the number of grab points, we save the
        // current transform data
        private void BeginTransform()
        {
            // End the transform on any existing transformer before we
            // begin the new one
            EndTransform();

            int useGrabPoints = _selectingPoints.Count;
            if (_maxGrabPoints != -1)
            {
                useGrabPoints = Mathf.Min(useGrabPoints, _maxGrabPoints);
            }

            switch (useGrabPoints)
            {
                case 1:
                    _activeTransformer = OneGrabTransformer;
                    break;
                case 2:
                    _activeTransformer = TwoGrabTransformer;
                    break;
                default:
                    _activeTransformer = null;
                    break;
            }

            if (_activeTransformer == null)
            {
                return;
            }

            _activeTransformer.BeginTransform();
        }

        private void UpdateTransform()
        {
            if (_activeTransformer == null)
            {
                return;
            }

            _activeTransformer.UpdateTransform();
        }

        private void EndTransform()
        {
            if (_activeTransformer == null)
            {
                return;
            }
            _activeTransformer.EndTransform();
            _activeTransformer = null;
        }

        #region Inject
        /// <summary>
        /// Injects an optional one-hand transformer component to be used when the object is grabbed with one hand.
        /// </summary>
        /// <param name="transformer">The transformer component that defines how the object behaves when grabbed with one hand.</param>
        public void InjectOptionalOneGrabTransformer(ITransformer transformer)
        {
            _oneGrabTransformer = transformer as UnityEngine.Object;
            OneGrabTransformer = transformer;
        }

        /// <summary>
        /// Injects an optional two-hand transformer component to be used when the object is grabbed with two hands.
        /// </summary>
        /// <param name="transformer">The transformer component that defines how the object behaves when grabbed with two hands.</param>
        public void InjectOptionalTwoGrabTransformer(ITransformer transformer)
        {
            _twoGrabTransformer = transformer as UnityEngine.Object;
            TwoGrabTransformer = transformer;
        }

        /// <summary>
        /// Sets an optional target transform to which all transformations will be applied, instead of the object this component is attached to.
        /// </summary>
        /// <param name="targetTransform">The transform component that should receive all transformations.</param>
        public void InjectOptionalTargetTransform(Transform targetTransform)
        {
            _targetTransform = targetTransform;
        }

        /// <summary>
        /// Injects an optional Rigidbody component to be controlled by this <see cref="Grabbable"/> object.
        /// </summary>
        /// <param name="rigidbody">The Rigidbody component to be manipulated during grab interactions.</param>
        public void InjectOptionalRigidbody(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
        }

        /// /// <summary>
        /// Configures whether the object should simulate throwing physics when unselected based on user interactions.
        /// </summary>
        /// <param name="throwWhenUnselected">A boolean value indicating whether to apply throwing dynamics when the object is released.</param>
        public void InjectOptionalThrowWhenUnselected(bool throwWehenUnselected)
        {
            _throwWhenUnselected = throwWehenUnselected;
        }

        /// <summary>
        /// Determines whether the Rigidbody should be locked to kinematic mode while the object is selected.
        /// </summary>
        /// <param name="kinematicWhileSelected">A boolean value indicating whether to lock the Rigidbody to kinematic mode during selection.</param>
        public void InjectOptionalKinematicWhileSelected(bool kinematicWhileSelected)
        {
            _kinematicWhileSelected = kinematicWhileSelected;
        }

        #endregion

        /// <summary>
        /// Tracks the movement of a rigidbody while it is selected by an <see cref="IPointable"/>
        /// and applies a throw velocity when it becomes fully unselected.
        /// </summary>
        private class ThrowWhenUnselected : ITimeConsumer, IDisposable
        {
            private Rigidbody _rigidbody;
            private IPointable _pointable;
            private Grabbable _grabbable;

            private HashSet<int> _selectors;

            private Func<float> _timeProvider = () => Time.time;
            public void SetTimeProvider(Func<float> timeProvider)
            {
                _timeProvider = timeProvider;
            }

            private static IObjectPool<RANSACVelocity> _ransacVelocityPool = new ObjectPool<RANSACVelocity>(
                createFunc: () => new RANSACVelocity(10, 2),
                collectionCheck: false,
                defaultCapacity: 2);

            private static IObjectPool<HashSet<int>> _selectorsPool = new ObjectPool<HashSet<int>>(
                createFunc: () => new HashSet<int>(),
                actionOnRelease: (s) => s.Clear(),
                collectionCheck: false,
                defaultCapacity: 2);

            private RANSACVelocity _ransacVelocity = null;

            private Pose _prevPose = Pose.identity;
            private float _prevTime = 0f;
            private bool _isHighConfidence = true;

            /// <summary>
            /// Creates a new instance that listens to the provided IPointable events.
            /// Note that this instance must be disposed via .Dispose() to release the event listener.
            /// </summary>
            /// <param name="rigidbody">The rigidbody to track velocity from and throw.</param>
            /// <param name="pointable">The IPointable indicating when the rigidbody is selected and unselected.</param>
            public ThrowWhenUnselected(Rigidbody rigidbody, Grabbable grabbable)
            {
                _rigidbody = rigidbody;
                _grabbable = grabbable;
                _pointable = grabbable; // Grabbable implements IPointable
                _pointable.WhenPointerEventRaised += HandlePointerEventRaised;
            }

            /// <summary>
            /// Unregisters the instance from the IPointable events
            /// </summary>
            public void Dispose()
            {
                _pointable.WhenPointerEventRaised -= HandlePointerEventRaised;
            }

            private void AddSelection(int selectorId)
            {
                if (_selectors == null)
                {
                    Initialize();
                }

                _selectors.Add(selectorId);
            }

            private void RemoveSelection(int selectorId, bool canThrow)
            {
                _selectors.Remove(selectorId);
                if (_selectors.Count == 0)
                {
                    if (canThrow)
                    {
                        //During Unselection, a Move call is executed storing the
                        //previous frame data, then the Target is moved to the final pose
                        //and the Unselect is invoked. At this point the Target position can
                        //be expected (in the general cases) to be certain, so we can process
                        //the data as if it happened this frame.
                        Process(false);
                        LoadThrowVelocities();
                    }
                    Teardown();
                }
            }

            private void HandlePointerEventRaised(PointerEvent evt)
            {
                switch (evt.Type)
                {
                    case PointerEventType.Select:
                        AddSelection(evt.Identifier);
                        break;
                    case PointerEventType.Move:
                        if (_selectors != null &&
                            _selectors.Contains(evt.Identifier))
                        {
                            //Move is invoked before the actual Transformer is applied to the target.
                            //Additionally several Move events can be fired per frame when grabbing
                            //with multiple points.
                            //So the pose of the target is still one frame behind, and we should store it
                            //as the previous frame data, not this one.
                            Process(true);
                            MarkFrameConfidence(evt.Identifier);
                        }
                        break;
                    case PointerEventType.Cancel:
                        RemoveSelection(evt.Identifier, false);
                        break;
                    case PointerEventType.Unselect:
                        MarkFrameConfidence(evt.Identifier);
                        RemoveSelection(evt.Identifier, true);
                        break;
                }
            }

            private void Initialize()
            {
                _selectors = _selectorsPool.Get();
                _ransacVelocity = _ransacVelocityPool.Get();
                _ransacVelocity.Initialize();
            }

            private void Teardown()
            {
                _selectorsPool.Release(_selectors);
                _selectors = null;
                _ransacVelocityPool.Release(_ransacVelocity);
                _ransacVelocity = null;
            }

            private void MarkFrameConfidence(int emitterKey)
            {
                if (!_isHighConfidence)
                {
                    return;
                }

                if (HandTrackingConfidenceProvider.TryGetTrackingConfidence(emitterKey,
                    out bool isHighConfidence))
                {
                    if (!isHighConfidence)
                    {
                        _isHighConfidence = false;
                    }
                }
            }

            private void Process(bool saveAsPreviousFrame)
            {
                float time = _timeProvider.Invoke();
                Pose pose = _rigidbody.transform.GetPose();

                if (time > _prevTime || !saveAsPreviousFrame)
                {
                    float frameTime = saveAsPreviousFrame ? _prevTime : time;
                    _isHighConfidence &= pose.position != _prevPose.position;
                    _ransacVelocity.Process(pose, frameTime, _isHighConfidence);
                    _isHighConfidence = true;
                }

                _prevTime = time;
                _prevPose = pose;
            }

            private void LoadThrowVelocities()
            {
                _ransacVelocity.GetVelocities(out Vector3 velocity, out Vector3 torque);
                velocity *= _grabbable._throwVelocityMultiplier;
                torque *= _grabbable._throwAngularVelocityMultiplier;
#pragma warning disable CS0618 // Type or member is obsoletes
                _rigidbody.velocity = velocity;
#pragma warning restore CS0618 // Type or member is obsolete
                _rigidbody.angularVelocity = torque;
            }
        }
    }
}
