# Account creation

Generated from the state machine definition. Do not edit by hand - run the tests to regenerate it.

```mermaid
stateDiagram-v2
    [*] --> Anonymous
    Anonymous --> Invited: Import
    Anonymous --> Registered: SignUp [not verified by OAuth]
    Anonymous --> Activated: SignUp [verified by OAuth]
    Registered --> Registered: SignUp
    Invited --> GroupMember: SignUp [on DrunkenKnitwits, presented with the invitation token]
    Invited --> Registered: SignUp [on DrunkenKnitwits, not presented with the invitation token]
    Invited --> Registered: SignUp [on Default]
    Registered --> Activated: Activate
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
| Invited | SignUp | GroupMember | on DrunkenKnitwits, presented with the invitation token | - |
| Invited | SignUp | Registered | on DrunkenKnitwits, not presented with the invitation token | - |
| Invited | SignUp | Registered | on Default | - |
| Registered | Activate | Activated | - | - |
| Activated | Join | PendingApproval | requiring approval | - |
| Activated | Join | GroupMember | not requiring approval | - |
| PendingApproval | Approve | GroupMember | - | - |
