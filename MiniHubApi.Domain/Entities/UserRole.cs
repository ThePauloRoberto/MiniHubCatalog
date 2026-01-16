namespace MiniHubApi.Domain.Entities;

public class UserRole
{
        public const string Admin = "Admin";
        public const string Editor = "Editor";
        public const string Viewer = "Viewer";

        public static List<string> GetAllRoles()
        {
            return new List<string> { Admin, Editor, Viewer };
        }

        public static Dictionary<string, string> GetRoleDescriptions()
        {
            return new Dictionary<string, string>
            {
                { Admin, "Administrador completo do sistema" },
                { Editor, "Pode criar e editar, mas não excluir" },
                { Viewer, "Somente visualização de dados" }
            };
        }
}