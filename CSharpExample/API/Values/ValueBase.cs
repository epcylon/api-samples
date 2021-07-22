using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BridgeRock.CSharpExample.API.Values
{
    /// <summary>
    /// The base class for all values (mainly just a basic INotifyPropertyChanged implementation).
    /// </summary>
    public abstract class ValueBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Notifies when one of the properties changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <remarks>
        /// The CallerMemberName attribute that is applied to the optional propertyName  
        /// parameter causes the property name of the caller to be substituted as an argument.  
        /// </remarks>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
