using System;

namespace org.jmrtd.cert
{
    public class CVCAuthorizationTemplate
    {
        public enum Role
        {
            CVCA,
            DV_DOMESTIC,
            DV_FOREIGN,
            AUTHENTICATION_TERMINAL,
            SIGNATURE_TERMINAL,
            IS
        }

        public enum Permission
        {
            READ_ACCESS_NONE,
            READ_ACCESS_DG3,
            READ_ACCESS_DG4,
            READ_ACCESS_DG3_AND_DG4
        }

        private readonly Role role;
        private readonly Permission permission;

        public CVCAuthorizationTemplate()
        {
            role = Role.IS;
            permission = Permission.READ_ACCESS_NONE;
        }

        public CVCAuthorizationTemplate(Role role, Permission permission)
        {
            this.role = role;
            this.permission = permission;
        }

        public Role GetRole() => role;
        public Permission GetPermission() => permission;

        public static Role FromRole(string roleString)
        {
            return roleString switch
            {
                "CVCA" => Role.CVCA,
                "DV_DOMESTIC" => Role.DV_DOMESTIC,
                "DV_FOREIGN" => Role.DV_FOREIGN,
                "AUTHENTICATION_TERMINAL" => Role.AUTHENTICATION_TERMINAL,
                "SIGNATURE_TERMINAL" => Role.SIGNATURE_TERMINAL,
                "IS" => Role.IS,
                _ => Role.IS
            };
        }

        public static Permission FromPermission(string permissionString)
        {
            return permissionString switch
            {
                "READ_ACCESS_NONE" => Permission.READ_ACCESS_NONE,
                "READ_ACCESS_DG3" => Permission.READ_ACCESS_DG3,
                "READ_ACCESS_DG4" => Permission.READ_ACCESS_DG4,
                "READ_ACCESS_DG3_AND_DG4" => Permission.READ_ACCESS_DG3_AND_DG4,
                _ => Permission.READ_ACCESS_NONE
            };
        }

        public override string ToString()
        {
            return $"CVCAuthorizationTemplate[{role}:{permission}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var other = (CVCAuthorizationTemplate)obj;
            return role == other.role && permission == other.permission;
        }

        public override int GetHashCode()
        {
            return role.GetHashCode() ^ permission.GetHashCode();
        }
    }
}
