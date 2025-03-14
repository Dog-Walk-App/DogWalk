using Microsoft.AspNetCore.SignalR;

namespace DogWalk_API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int usuarioId, int paseadorId, string mensaje)
        {
            // Crear un grupo único para la conversación entre usuario y paseador
            string groupName = GetGroupName(usuarioId, paseadorId);
            
            // Enviar mensaje al grupo
            await Clients.Group(groupName).SendAsync("ReceiveMessage", usuarioId, paseadorId, mensaje, DateTime.Now);
        }

        public async Task JoinChat(int usuarioId, int paseadorId)
        {
            string groupName = GetGroupName(usuarioId, paseadorId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveChat(int usuarioId, int paseadorId)
        {
            string groupName = GetGroupName(usuarioId, paseadorId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private string GetGroupName(int usuarioId, int paseadorId)
        {
            // Asegurar que el nombre del grupo sea consistente independientemente del orden de los IDs
            return usuarioId < paseadorId
                ? $"chat_{usuarioId}_{paseadorId}"
                : $"chat_{paseadorId}_{usuarioId}";
        }
    }
}