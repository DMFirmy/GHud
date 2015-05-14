namespace GHud.DataStructures
{
	/// <summary>
	/// This data structure represents the orbit of a single vessel around a celestial body.
	/// </summary>
	public struct OrbitData
	{
		#region Constructors
#if !DEBUG
		/// <summary>
		/// Creates a new OrbitData instance representing the supplied orbit.
		/// </summary>
		/// <param name="orbit">The orbit that this orbit data represents.</param>
		public OrbitData(Orbit orbit)
		{
			// Orbit info
			_apR = orbit.ApR;
			_peR = orbit.PeR;
			_apA = orbit.ApA;
			_peA = orbit.PeA;

			// Calc the diameter of the reference body and the diameter of its atmosphere
			_bodyDiameter = orbit.referenceBody.Radius*2;
			_atmosphereDiameter = _bodyDiameter + (orbit.referenceBody.atmosphereDepth*2);

			_semiMajorAxis = orbit.semiMajorAxis;
			_semiMinorAxis = orbit.semiMinorAxis;
			_eccentricity = orbit.eccentricity;
			_inclination = orbit.inclination;

			_trueAnomaly = orbit.trueAnomaly;
			_radiusAtTrueAnomaly = orbit.RadiusAtTrueAnomaly(orbit.trueAnomaly);

			_velocity = orbit.vel.magnitude;

			_timeToApoapsis = orbit.timeToAp;
			_timeToPeriapsis = orbit.timeToPe;

			_orbitedBodyName = orbit.referenceBody.GetName();
		}
#endif
		#endregion

		#region Fields
		private double _apA;
		private double _apR;
		private double _atmosphereDiameter;
		private double _bodyDiameter;
		private double _eccentricity;
		private double _inclination;
		private string _orbitedBodyName;
		private double _peA;
		private double _peR;
		private double _radiusAtTrueAnomaly;
		private double _semiMajorAxis;
		private double _semiMinorAxis;
		private double _timeToApoapsis;
		private double _timeToPeriapsis;
		private double _trueAnomaly;
		private double _velocity;
		#endregion

		#region Properties
		public double ApA
		{
			get { return _apA; }
			set { _apA = value; }
		}

		public double ApR
		{
			get { return _apR; }
			set { _apR = value; }
		}

		public double PeA
		{
			get { return _peA; }
			set { _peA = value; }
		}

		public double PeR
		{
			get { return _peR; }
			set { _peR = value; }
		}

		public double BodyDiameter
		{
			get { return _bodyDiameter; }
			set { _bodyDiameter = value; }
		}

		public double AtmosphereDiameter
		{
			get { return _atmosphereDiameter; }
			set { _atmosphereDiameter = value; }
		}

		public double SemiMajorAxis
		{
			get { return _semiMajorAxis; }
			set { _semiMajorAxis = value; }
		}

		public double SemiMinorAxis
		{
			get { return _semiMinorAxis; }
			set { _semiMinorAxis = value; }
		}

		public double Eccentricity
		{
			get { return _eccentricity; }
			set { _eccentricity = value; }
		}

		public double Inclination
		{
			get { return _inclination; }
			set { _inclination = value; }
		}

		public double TrueAnomaly
		{
			get { return _trueAnomaly; }
			set { _trueAnomaly = value; }
		}

		public double RadiusAtTrueAnomaly
		{
			get { return _radiusAtTrueAnomaly; }
			set { _radiusAtTrueAnomaly = value; }
		}

		public double Velocity
		{
			get { return _velocity; }
			set { _velocity = value; }
		}

		public double TimeToApoapsis
		{
			get { return _timeToApoapsis; }
			set { _timeToApoapsis = value; }
		}

		public double TimeToPeriapsis
		{
			get { return _timeToPeriapsis; }
			set { _timeToPeriapsis = value; }
		}

		public string OrbitedBodyName
		{
			get { return _orbitedBodyName; }
			set { _orbitedBodyName = value; }
		}
		#endregion
	}
}