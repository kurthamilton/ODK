# Account creation

Generated from the state machine definition. Do not edit by hand - run the tests to regenerate it.

```mermaid
stateDiagram-v2
    [*] --> Anonymous
    Anonymous --> Invited: Import
    Anonymous --> Registered: SignUp [not verified by OAuth]
    Anonymous --> Activated: SignUp [verified by OAuth]
    Registered --> Registered: SignUp
    Invited --> Registered: SignUp [on DrunkenKnitwits, presented with the invitation token]
    Invited --> Registered: SignUp [on DrunkenKnitwits, not presented with the invitation token]
    Invited --> Invited: SignUp [on Default]
    Invited --> Activated: Activate
    Registered --> Activated: Activate [not a member of the group]
    Registered --> GroupMember: Activate [a member of the group, approved]
    Registered --> PendingApproval: Activate [a member of the group, not approved]
    Activated --> PendingApproval: Join [requiring approval]
    Activated --> GroupMember: Join [not requiring approval]
    PendingApproval --> GroupMember: Approve
```

## Transitions

| From | Trigger | To | When | Steps |
| --- | --- | --- | --- | --- |
| Anonymous | Import | Invited | - | - |
| Anonymous | SignUp | Registered | not verified by OAuth | - |
| Anonymous | SignUp | Activated | verified by OAuth | - |
| Registered | SignUp | Registered | - | - |
| Invited | SignUp | Registered | on DrunkenKnitwits, presented with the invitation token | - |
| Invited | SignUp | Registered | on DrunkenKnitwits, not presented with the invitation token | - |
| Invited | SignUp | Invited | on Default | - |
| Invited | Activate | Activated | - | - |
| Registered | Activate | Activated | not a member of the group | - |
| Registered | Activate | GroupMember | a member of the group, approved | - |
| Registered | Activate | PendingApproval | a member of the group, not approved | - |
| Activated | Join | PendingApproval | requiring approval | 1. checks the group has room for another member (Decision)<br>2. checks the group's required questions are answered (Decision)<br>3. adds the member to the group (Write)<br>4. consumes the invitation (Write)<br>5. notifies the group's admins in the app (Write)<br>6. commits the changes (Commit)<br>7. emails the group's admins (ExternalEffect) |
| Activated | Join | GroupMember | not requiring approval | 1. checks the group has room for another member (Decision)<br>2. checks the group's required questions are answered (Decision)<br>3. adds the member to the group (Write)<br>4. consumes the invitation (Write)<br>5. notifies the group's admins in the app (Write)<br>6. commits the changes (Commit)<br>7. emails the group's admins (ExternalEffect) |
| PendingApproval | Approve | GroupMember | - | - |
