using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RiseProject.Tomis.CustomTypes
{
	/// <summary>
	/// This interface should be implemented in such a way that Instantiate should be forced to be called.
	/// IsInstantiated should be set to true when Instantiate is called
	/// </summary>
	public interface IInstantiatable {

		bool IsInstantiated { get; set; }
		void Instantiate();
        void Disable();
	}
}
