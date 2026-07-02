# VolunteerHQ

A web platform for coordinating volunteers, volunteer organizations, and military units around fundraisers.

**Stack:** ASP.NET Core 10 · Entity Framework Core 10 · PostgreSQL · SignalR · React (Vite) · Tailwind CSS

## Quick Start (Docker)

The only prerequisite is [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/macOS) or Docker Engine with the Compose plugin (Linux).

```bash
git clone https://github.com/m1nnesang/VolunteerHQProject.git
cd VolunteerHQProject
docker compose up --build
```

The first build takes a few minutes. When the containers are up, open:

- **App:** http://localhost:8080

That's it — the database schema is created automatically on startup. Register a new account through the UI to explore the app.

Useful commands:

```bash
docker compose up -d          # run in the background
docker compose down           # stop
docker compose down -v        # stop and wipe the database
```

What runs inside:

| Container | Image | Purpose |
|---|---|---|
| `client` | nginx (serves the built React SPA) | Frontend on port 8080, proxies `/api` and `/hubs` to the API |
| `api` | ASP.NET Core 10 | REST API + SignalR hub |
| `db` | PostgreSQL 17 | Database (data persists in the `pgdata` volume) |

### Configuration (optional)

The stack starts with safe defaults — no configuration is required. To override secrets (SMTP credentials for email notifications, Nova Poshta API key, JWT signing key, database password), copy the example env file and fill in the values:

```bash
cp .env.example .env
docker compose up -d --build
```

| Variable | Purpose | Default |
|---|---|---|
| `POSTGRES_PASSWORD` | Database password | `volunteerhq` |
| `JWT_KEY` | JWT signing key | local demo key |
| `SMTP_USERNAME` / `SMTP_PASSWORD` | Gmail SMTP credentials for email notifications | empty (emails silently skipped) |
| `NOVAPOSHTA_API_KEY` | Nova Poshta API key | empty |

`.env` is gitignored — secrets never land in the repository.

## Solution Structure

- **VolunteerHQ.Core** - domain models, DTOs, enums, exceptions (no external dependencies)
- **VolunteerHQ.Infrastructure** - services (business logic), `AppDbContext`, realtime notifier implementation
- **VolunteerHQ.API** - REST controllers, middleware, SignalR hub (`/hubs/chat`)
- **VolunteerHQ.Tests** - unit tests (xUnit + FluentAssertions + Moq + EF Core InMemory)
- **VolunteerHQ.Client** - React frontend (SPA)

## ER Diagram

```mermaid
erDiagram
    User {
        int Id PK
        string Email UK
        string PasswordHash
        string FirstName
        string SecondName
        date BirthDate
        string City
        string AvatarPath
        int Role
        bool IsBanned
        string BanReason
        datetime BannedAt
        datetime CreatedAt
    }

    VolunteerProfile {
        int Id PK
        int UserId FK
        string Bio
        string CvFilePath
        string Skills
        string Experience
        datetime CreatedAt
    }

    Organization {
        int Id PK
        string OrganizationName
        string City
        string Description
        datetime CreatedAt
    }

    OrganizationMembership {
        int Id PK
        int UserId FK
        int OrganizationId FK
        int MemberRole
        datetime JoinedAt
    }

    JoinRequest {
        int Id PK
        int UserId FK
        int OrganizationId FK
        string Bio
        string Skills
        string Experience
        string CvFilePath
        int Status
        datetime CreatedAt
        datetime ReviewedAt
        int ReviewedByUserId FK
    }

    OrganizationRequest {
        int Id PK
        int UserId FK
        string Bio
        string Experience
        string Skills
        string CvFilePath
        string ProposedName
        string City
        string Description
        int Status
        datetime CreatedAt
        datetime ReviewedAt
        int ReviewedByUserId FK
        string AdminComment
    }

    MilitaryUnit {
        int Id PK
        string Login UK
        string PasswordHash
        string UnitName
        string ContactPersonName
        bool IsNameHidden
        datetime CreatedAt
    }

    Fundraiser {
        int Id PK
        int MilitaryUnitId FK
        string Title
        string Description
        decimal TotalGoal
        int Importance
        date Deadline
        int Status
        datetime CreatedAt
    }

    FundraiserAssignment {
        int Id PK
        int FundraiserId FK
        int OrganizationId FK
        string UniqueCode UK
        datetime TakenAt
    }

    Donation {
        int Id PK
        int FundraiserId FK
        int FundraiserAssignmentId FK
        int UserId FK
        decimal Amount
        bool IsAnonymous
        datetime CreatedAt
    }

    Comment {
        int Id PK
        int UserId FK
        int FundraiserId FK
        string Text
        datetime CreatedAt
    }

    PrivateMessage {
        int Id PK
        int SenderId FK
        int ReceiverId FK
        string Text
        bool IsRead
        bool IsEdited
        datetime SentAt
    }

    Subscription {
        int Id PK
        int UserId FK
        int Target
        int TargetId
        datetime SubscribedAt
    }

    Notification {
        int Id PK
        int UserId FK
        string Text
        string Link
        bool IsRead
        datetime SentAt
    }

    Report {
        int Id PK
        int ReporterId FK
        int ReportedId FK
        string Reason
        int Category
        int Status
        datetime CreatedAt
        datetime ReviewedAt
        string AdminComment
    }

    AuditLog {
        int Id PK
        int UserId FK
        string Action
        string EntityType
        int EntityId
        string Details
        datetime CreatedAt
    }

    RefreshToken {
        int Id PK
        int UserId FK
        string Token
        datetime ExpiresAt
        bool IsRevoked
        datetime CreatedAt
    }

    User ||--o| VolunteerProfile : "has profile"
    User ||--o{ OrganizationMembership : "is member"
    Organization ||--o{ OrganizationMembership : "has members"
    User ||--o{ JoinRequest : "submits"
    Organization ||--o{ JoinRequest : "receives"
    User ||--o{ OrganizationRequest : "submits"
    MilitaryUnit ||--o{ Fundraiser : "creates"
    Fundraiser ||--o{ FundraiserAssignment : "assigned via"
    Organization ||--o{ FundraiserAssignment : "joins"
    Fundraiser ||--o{ Donation : "receives"
    FundraiserAssignment ||--o{ Donation : "tracks"
    User ||--o{ Donation : "donates"
    User ||--o{ Comment : "writes"
    Fundraiser ||--o{ Comment : "has"
    User ||--o{ PrivateMessage : "sends"
    User ||--o{ PrivateMessage : "receives"
    User ||--o{ Subscription : "subscribes"
    User ||--o{ Notification : "receives"
    User ||--o{ Report : "reports"
    User ||--o{ Report : "is reported"
    User ||--o{ AuditLog : "performs"
    User ||--o{ RefreshToken : "owns"
```

### Enum field explanations

| Field | Enum | Values |
|---|---|---|
| `User.Role` | `UserRoles` | User, Volunteer, Admin |
| `OrganizationMembership.MemberRole` | `OrganizationMemberRole` | Leader, Deputy, Member |
| `JoinRequest.Status`, `OrganizationRequest.Status` | `RequestStatus` | Pending, Approved, Rejected |
| `Fundraiser.Status` | `FundraiserStatus` | Open, InProgress, Completed, Closed |
| `Fundraiser.Importance` | `FundraiserImportance` | Low, Medium, High, Critical |
| `Subscription.Target` | `SubscriptionTargetType` | Organization, MilitaryUnit |
| `Report.Category` | `ReportCategory` | Spam, Abuse, Fraud, Other |
| `Report.Status` | `ReportStatus` | Pending, Reviewed, Dismissed |

### Data model notes

- **`Fundraiser.CurrentProgress`** and **`FundraiserAssignment.AmountRaised`** are not stored in the database - they are computed on the fly as `SUM(Donations.Amount)`. This eliminates desync between aggregated totals and actual donation records.
- **`Donation.UserId`** and **`Donation.FundraiserAssignmentId`** are nullable: a donation can be made anonymously and/or without being tied to a specific organization (a direct donation on the fundraiser page).
- **`PrivateMessage.SenderId`** and **`ReceiverId`** are nullable with `OnDelete(SetNull)`: deleting a user does not cascade-delete their entire message history.
- **`RefreshToken.IsRevoked`** - each time an access token is refreshed, the old refresh token is invalidated (rotation).
