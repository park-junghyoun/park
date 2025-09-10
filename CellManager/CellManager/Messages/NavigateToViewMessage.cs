using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class NavigateToViewMessage : ValueChangedMessage<Type>
    {
        public NavigateToViewMessage(Type viewModelType) : base(viewModelType)
        {
        }
    }
}
