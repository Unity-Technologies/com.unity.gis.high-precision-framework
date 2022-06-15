using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;
using UnityEngine;


public class MovingCamera : MonoBehaviour
{
    /// <summary>
    /// Midpoint that the camera first transitions to, before moving to the final desired location
    /// </summary>
    [SerializeField]
    private HPTransform m_TransitionMidPoint;

    /// <summary>
    /// List of HPTransforms that record the positions that the camera can transition to
    /// </summary>
    [SerializeField]
    private List<HPTransform> m_Locations;

    /// <summary>
    /// Proximity threshold relative to the position of m_TransitionMidPoint
    /// </summary>
    [SerializeField]
    private float m_RequiredProximityToMidPoint = 1000000f;

    /// <summary>
    /// Proximity threshold relative to a location (i.e. a location in m_Locations)
    /// </summary>
    [SerializeField]
    private float m_RequiredProximityToLocation = 1f;

    /// <summary>
    /// Camera moving speed
    /// </summary>
    [SerializeField]
    private float m_Speed = 2.0f;

    private bool m_TransitionInProgress = false;
    private HPTransform m_HPTransform;

    private void OnEnable()
    {
        m_HPTransform = GetComponent<HPTransform>();
    }

    /// <summary>
    /// Start the transition of the camera to a specific indexed location
    /// </summary>
    /// <param name="locationIndex"></param>
    public void StartTransition(int locationIndex)
    {
        Task _ = Transition(locationIndex);
    }

    /// <summary>
    /// Bring the camera to a specific indexed location
    /// </summary>
    /// <param name="locationIndex"></param>
    /// <returns></returns>
    private async Task Transition(int locationIndex)
    {
        if (m_TransitionInProgress)
        {
            Debug.Log("Transition is already in progress");
            return;
        }

        if (locationIndex < 0 || locationIndex >= m_Locations.Count)
        {
            Debug.LogError("Invalid index provided");
            return;
        }
        
        m_TransitionInProgress = true;

        await Task.Yield();

        HPTransform target = m_Locations[locationIndex];

        await TransitionTo(m_TransitionMidPoint.UniversePosition, m_TransitionMidPoint.UniverseRotation, m_RequiredProximityToMidPoint);
        await TransitionTo(target.UniversePosition, target.UniverseRotation, m_RequiredProximityToLocation);

        m_TransitionInProgress = false;
    }

    /// <summary>
    /// Bring the camera close to a specific position and rotation within the input proximity threshold
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="requiredProximity"></param>
    /// <returns></returns>
    private async Task TransitionTo(double3 position, quaternion rotation, double requiredProximity)
    {
        quaternion originalRotation = m_HPTransform.UniverseRotation;

        double totalDistance = math.distance(m_HPTransform.UniversePosition, position);

        double currentDistance;
        while ((currentDistance = math.distance(m_HPTransform.UniversePosition, position)) > requiredProximity)
        {
            double3 direction = math.normalize(position - m_HPTransform.UniversePosition);
            double height = m_HPTransform.UniversePosition.y;
            double speed = m_Speed * height;

            double rotationCoefficient = 1.0 - (currentDistance / totalDistance);

            m_HPTransform.UniversePosition += speed * direction * Time.deltaTime;
            m_HPTransform.UniverseRotation = math.slerp(originalRotation, rotation, (float)rotationCoefficient);

            await Task.Yield();
        }
    }
}
