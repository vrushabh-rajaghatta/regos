import { Link } from "react-router-dom";

import type { UserListItem } from "../types/UserListItem";
import { UserStatusBadge } from "./UserStatusBadge";

interface UsersTableProps {
  users: UserListItem[];
}

export function UsersTable({ users }: UsersTableProps) {
  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm">
        <thead className="border-b bg-muted/50">
          <tr className="text-left">
            <th className="px-4 py-2.5 font-medium">Name</th>
            <th className="px-4 py-2.5 font-medium">Email</th>
            <th className="px-4 py-2.5 font-medium">Status</th>
            <th className="px-4 py-2.5 font-medium">Created On</th>
          </tr>
        </thead>

        <tbody>
          {users.map((user) => (
            <tr key={user.id} className="border-b last:border-0 hover:bg-muted/40">
              <td className="px-4 py-2.5 font-medium">
                <Link
                  to={`/platform/users/${user.id}`}
                  className="hover:underline"
                >
                  {user.firstName} {user.lastName}
                </Link>
              </td>

              <td className="px-4 py-2.5 text-muted-foreground">
                {user.email}
              </td>

              <td className="px-4 py-2.5">
                <UserStatusBadge status={user.status} />
              </td>

              <td className="px-4 py-2.5 text-muted-foreground">
                {new Date(user.createdOn).toLocaleDateString()}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
