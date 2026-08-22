# Account

Generated from the state machine definition. Do not edit by hand - run the tests to regenerate it.

```mermaid
stateDiagram-v2
    [*] --> Anonymous
    Anonymous --> Registered: Import
    Anonymous --> Registered: SignUp [to a group]
    Anonymous --> Registered: SignUp [not to a group, not verified by OAuth]
    Anonymous --> Activated: SignUp [not to a group, verified by OAuth]
    Registered --> Registered: SignUp [to a group, presented with the invitation token]
    Registered --> Registered: SignUp [to a group, not presented with the invitation token]
    Registered --> Registered: SignUp [not to a group]
    Activated --> Activated: SignUp
    Registered --> Activated: Activate [in a group]
    Registered --> Activated: Activate [not in a group]
    Registered --> Activated: AcceptInvite
```

## Transitions

| From | Trigger | To | When | Steps |
| --- | --- | --- | --- | --- |
| Anonymous | Import | Registered | - | 1. raises an account for the imported address (Write) |
| Anonymous | SignUp | Registered | to a group | 1. checks the group has room for another member (Decision)<br>2. checks the group's required questions and the email address (Decision)<br>3. checks the submitted picture is an image (Decision)<br>4. creates the account (Write)<br>5. applies the email opt-in choice (Write)<br>6. stores the member's locale (Write)<br>7. joins the group (Write)<br>8. places the member where the group is (Write)<br>9. puts the account on the default site subscription (Write)<br>10. stores the member's picture (Write)<br>11. issues the activation token (Write)<br>12. re-raises invitations to other groups (Write)<br>13. commits the sign-up (Commit)<br>14. emails the activation link (ExternalEffect) |
| Anonymous | SignUp | Registered | not to a group, not verified by OAuth | 1. checks the email address (Decision)<br>2. creates the account, and places it from the submitted location (Write)<br>3. puts the account on the default site subscription (Write)<br>4. records the member's interests (Write)<br>5. re-raises invitations to other groups (Write)<br>6. issues the activation token (Write)<br>7. commits the sign-up (Commit)<br>8. creates the interests the member typed in (ExternalEffect)<br>9. emails the activation link (ExternalEffect) |
| Anonymous | SignUp | Activated | not to a group, verified by OAuth | 1. checks the email address (Decision)<br>2. creates the account, and places it from the submitted location (Write)<br>3. puts the account on the default site subscription (Write)<br>4. records the member's interests (Write)<br>5. re-raises invitations to other groups (Write)<br>6. marks the account activated (Write)<br>7. commits the sign-up (Commit)<br>8. creates the interests the member typed in (ExternalEffect)<br>9. emails a welcome (ExternalEffect) |
| Registered | SignUp | Registered | to a group, presented with the invitation token | 1. checks the group has room for another member (Decision)<br>2. checks the group's required questions and the email address (Decision)<br>3. checks the submitted picture is an image (Decision)<br>4. discards the unactivated account being replaced (Write)<br>5. commits the changes (Commit)<br>6. creates the account (Write)<br>7. applies the email opt-in choice (Write)<br>8. stores the member's locale (Write)<br>9. joins the group (Write)<br>10. places the member where the group is (Write)<br>11. puts the account on the default site subscription (Write)<br>12. stores the member's picture (Write)<br>13. issues the activation token (Write)<br>14. re-raises invitations to other groups (Write)<br>15. commits the sign-up (Commit) |
| Registered | SignUp | Registered | to a group, not presented with the invitation token | 1. checks the group has room for another member (Decision)<br>2. checks the group's required questions and the email address (Decision)<br>3. checks the submitted picture is an image (Decision)<br>4. discards the unactivated account being replaced (Write)<br>5. commits the changes (Commit)<br>6. creates the account (Write)<br>7. applies the email opt-in choice (Write)<br>8. stores the member's locale (Write)<br>9. joins the group (Write)<br>10. places the member where the group is (Write)<br>11. puts the account on the default site subscription (Write)<br>12. stores the member's picture (Write)<br>13. issues the activation token (Write)<br>14. re-raises invitations to other groups (Write)<br>15. commits the sign-up (Commit)<br>16. emails the activation link (ExternalEffect) |
| Registered | SignUp | Registered | not to a group | 1. checks the email address (Decision)<br>2. discards the unactivated account being replaced (Write)<br>3. commits the changes (Commit)<br>4. creates the account, and places it from the submitted location (Write)<br>5. puts the account on the default site subscription (Write)<br>6. records the member's interests (Write)<br>7. re-raises invitations to other groups (Write)<br>8. issues the activation token (Write)<br>9. commits the sign-up (Commit)<br>10. creates the interests the member typed in (ExternalEffect)<br>11. emails the activation link (ExternalEffect) |
| Activated | SignUp | Activated | - | 1. emails the address to say it already has an account (ExternalEffect) |
| Registered | Activate | Activated | in a group | 1. checks the password is allowed (Decision)<br>2. marks the account activated (Write)<br>3. stores the password (Write)<br>4. spends the activation link (Write)<br>5. notifies the group of a new member (Write)<br>6. commits the changes (Commit)<br>7. emails the group about its new member (ExternalEffect) |
| Registered | Activate | Activated | not in a group | 1. checks the password is allowed (Decision)<br>2. marks the account activated (Write)<br>3. stores the password (Write)<br>4. spends the activation link (Write)<br>5. commits the changes (Commit)<br>6. emails a welcome (ExternalEffect) |
| Registered | AcceptInvite | Activated | - | 1. checks the password is allowed (Decision)<br>2. marks the account activated (Write)<br>3. stores the password (Write)<br>4. spends the activation link (Write)<br>5. records the name the member confirmed (Write)<br>6. joins the group the invitation was to (Write)<br>7. commits the changes (Commit)<br>8. emails the group about its new member (ExternalEffect) |
