using System;

namespace NMolecules.Architecture.Mvvm
{
    /// <summary>
    /// Identifies a view-model type in a Model-View-ViewModel (MVVM) architecture.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Struct)]
    public class ViewModelAttribute : Attribute
    {
    }
}
