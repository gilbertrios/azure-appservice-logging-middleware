# When to Split a Microservice: Comprehensive Guide

## Table of Contents
1. [Overview](#overview)
2. [Key Principles](#key-principles)
3. [Warning Signs](#warning-signs)
4. [Split Criteria](#split-criteria)
5. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
6. [Decision Framework](#decision-framework)
7. [Real-World Examples](#real-world-examples)
8. [Post-Split Validation](#post-split-validation)

---

## Overview

**Core Philosophy:** Start with larger services (modular monoliths) and split only when organizational, technical, or business needs demand it.

### The Golden Rule
> "Don't split microservices based on endpoint count alone. Split based on bounded contexts, team ownership, and deployment independence."

### Recommended Service Sizes
- **5-15 endpoints** = Healthy microservice ✅
- **15-25 endpoints** = Warning zone ⚠️ (evaluate split criteria)
- **25+ endpoints** = Likely needs splitting 🚩

---

## Key Principles

### 1. **Bounded Context (Domain-Driven Design)**

Split when you have distinct business capabilities that can evolve independently.

```
❌ BAD: Mixed Responsibilities
UserOrderPaymentService (30 endpoints):
- User CRUD
- Order processing
- Payment processing
- All tangled together

✅ GOOD: Clear Boundaries
UserService (8 endpoints):
- User profile management
- Authentication
- User preferences

OrderService (12 endpoints):
- Order creation & management
- Order status tracking
- Order history

PaymentService (6 endpoints):
- Payment processing
- Refunds
- Transaction history
```

**Split Indicator:** Can you explain the service's purpose in one sentence?
- ✅ "Manages user profiles and authentication"
- ❌ "Handles users, orders, payments, and some inventory stuff"

---

### 2. **Team Ownership**

Split when multiple teams need to modify the same service regularly.

```
Scenario: E-commerce Platform

Single Service (40 endpoints):
- Checkout Team needs to update payment flow
- Catalog Team needs to update product search
- Order Team needs to update fulfillment logic
→ Constant merge conflicts, release coordination nightmares

Split Services:
CheckoutService → Checkout Team (12 endpoints)
CatalogService → Catalog Team (15 endpoints)
OrderService → Order Team (13 endpoints)
→ Teams can deploy independently
```

**Split Indicator:**
- 🚩 3+ teams making PRs to same service weekly
- 🚩 "Who owns this code?" questions arise frequently
- 🚩 Release requires approval from multiple teams

**Team Size Rule:**
- 1 team (2-8 people) = 1-2 microservices
- Multiple teams = Separate services per team

---

### 3. **Deployment Independence**

Split when you need to deploy features without coordinating across teams.

```
❌ Coupled Deployment:
"We can't deploy the new search algorithm because 
the checkout team isn't ready with their payment changes"

✅ Independent Deployment:
Search Service:
- Deploy search improvements 10x per week
- No coordination needed
- Rollback only affects search

Checkout Service:
- Deploy payment updates once per month
- Heavily tested, compliance-reviewed
- Independent release schedule
```

**Split Indicator:**
- 🚩 Can't deploy feature X without deploying feature Y
- 🚩 Release cadence mismatch (some features need rapid iteration)
- 🚩 Hotfixes require testing unrelated functionality

---

### 4. **Data Ownership**

Split when different parts of the service need different data models or databases.

```
✅ GOOD: Clear Data Boundaries

OrderService:
Database: OrderDB
Tables: Orders, OrderItems, OrderStatus
Owns: All order-related data
Never directly queries: User or Payment tables

UserService:
Database: UserDB
Tables: Users, Profiles, Preferences
Owns: All user-related data
Never directly queries: Order or Payment tables

Communication: Via APIs or events, not shared database
```

**Split Indicator:**
- 🚩 Service queries 5+ different database schemas
- 🚩 Different parts need different database technologies (SQL vs NoSQL)
- 🚩 Data access patterns conflict (OLTP vs OLAP)
- 🚩 Different data residency/compliance requirements

---

### 5. **Scaling Requirements**

Split when different functionalities have vastly different scaling needs.

```
Scenario: E-commerce Platform

Product Catalog:
- Read-heavy (10,000 requests/min)
- Write-light (10 updates/min)
- Needs 20 instances
- Aggressive caching (Redis)
- CDN integration

Order Processing:
- Write-heavy (100 orders/min)
- Read-moderate (500 requests/min)
- Needs 5 instances
- Transaction consistency critical
- No aggressive caching

✅ Split allows:
- Catalog: Scale horizontally with read replicas
- Orders: Optimize for write consistency
```

**Split Indicator:**
- 🚩 Some endpoints need 10x more instances than others
- 🚩 Different caching strategies needed
- 🚩 Some features are CPU-intensive, others are I/O-bound
- 🚩 Cost inefficiency (paying for unused capacity)

---

### 6. **Different SLAs & Reliability Requirements**

Split when parts of the service have different uptime or performance requirements.

```
User Profile Service:
- SLA: 99.9% (43 min downtime/month acceptable)
- Response time: <500ms is fine
- Deploy: During business hours OK
- Downtime impact: Users see cached data

Payment Processing Service:
- SLA: 99.99% (4 min downtime/month max)
- Response time: <100ms required
- Deploy: Zero-downtime only
- Downtime impact: Lost revenue, regulatory issues
- PCI-DSS compliance required

✅ Split allows:
- Different deployment strategies
- Different infrastructure (Premium vs Basic)
- Different monitoring/alerting thresholds
```

**Split Indicator:**
- 🚩 Some features are "critical" while others are "nice to have"
- 🚩 Different compliance requirements (PCI-DSS, HIPAA, GDPR)
- 🚩 Different performance SLAs in contracts
- 🚩 Different disaster recovery requirements

---

### 7. **Technology Stack Differences**

Split when different parts need different technologies or frameworks.

```
Legacy Invoice Service:
- .NET Framework 4.8 (Windows only)
- SQL Server
- Cannot upgrade due to dependencies
- Must maintain for 3 years

New Analytics Service:
- .NET 8 (Linux containers)
- PostgreSQL + Redis
- Modern architecture
- Rapid feature development

✅ Split allows:
- Invoice Service: Stable, rarely changed
- Analytics Service: Modern stack, frequent updates
```

**Split Indicator:**
- 🚩 Different language/runtime requirements
- 🚩 Legacy code that can't be modernized
- 🚩 Some features need specific libraries/frameworks
- 🚩 Platform constraints (Windows vs Linux)

---

### 8. **Change Frequency & Blast Radius**

Split when some features change frequently while others are stable.

```
Marketing Campaigns API:
- Changes: Daily (new campaigns, A/B tests)
- Risk: High (experimental features)
- Failures: Acceptable (can show default content)
- Deploy: 20+ times per week

Core Transaction API:
- Changes: Monthly (well-tested, regulated)
- Risk: Very low (financial transactions)
- Failures: Unacceptable (money movement)
- Deploy: Once per month with extensive testing

✅ Split allows:
- Experiment rapidly on marketing without risking transactions
- Isolate failures
- Different testing rigor
```

**Split Indicator:**
- 🚩 Some features change 10x more frequently than others
- 🚩 Failures in one area shouldn't affect other areas
- 🚩 Different risk tolerance for changes
- 🚩 Some features are experimental, others are stable

---

## Warning Signs You Need to Split

### 🚩 Organizational Indicators

| Warning Sign | Severity | Example |
|--------------|----------|---------|
| **Multiple teams fighting over same codebase** | HIGH | 5+ merge conflicts per week between teams |
| **Unclear ownership** | HIGH | "Who maintains the payment endpoints?" |
| **Release coordination overhead** | HIGH | Need 4 teams to approve every deployment |
| **Long release cycles** | MEDIUM | "Can't deploy for 2 weeks due to Team B's changes" |
| **Cross-team dependencies** | MEDIUM | Team A blocked waiting for Team B |
| **Onboarding confusion** | LOW | New developers can't understand what service does |

### 🚩 Technical Indicators

| Warning Sign | Severity | Example |
|--------------|----------|---------|
| **Cascading failures** | HIGH | Search breaks → checkout goes down |
| **Tangled dependencies** | HIGH | Changed orders → broke inventory AND shipping |
| **Different scaling needs** | HIGH | Need 20 instances for reads, wasting CPU on writes |
| **Large codebase** | MEDIUM | Service has 200+ classes, 50k+ LOC |
| **Test suite too slow** | MEDIUM | Takes 30+ minutes to run all tests |
| **Deploy time > 30 minutes** | MEDIUM | Too much to deploy in one package |
| **Shared state conflicts** | MEDIUM | Feature A's cache invalidates Feature B's data |

### 🚩 Business Indicators

| Warning Sign | Severity | Example |
|--------------|----------|---------|
| **Different compliance requirements** | HIGH | Payment needs PCI-DSS, profile doesn't |
| **Different SLAs** | HIGH | Orders need 99.99%, reports need 99% |
| **Different customer segments** | MEDIUM | Internal APIs vs public partner APIs |
| **Different release schedules** | MEDIUM | Marketing: weekly, Finance: quarterly |
| **Cost inefficiency** | LOW | Running 24/7 for features used 1 hour/day |

---

## Decision Framework

### Step 1: Count the Red Flags

Go through the criteria above and count warning signs:

```
Score Your Service:

Organizational:
□ Multiple teams modifying service (3 points)
□ Unclear ownership (2 points)
□ Release coordination needed (2 points)

Technical:
□ Cascading failures (3 points)
□ Different scaling needs (3 points)
□ Large codebase >30k LOC (2 points)
□ Tangled dependencies (2 points)

Business:
□ Different compliance requirements (3 points)
□ Different SLAs (3 points)
□ Different release schedules (2 points)

Total Score: _____
```

**Scoring Guide:**
- **0-5 points:** Don't split (keep as modular monolith)
- **6-10 points:** Consider splitting (evaluate costs vs benefits)
- **11+ points:** Split immediately (technical debt building)

---

### Step 2: Apply the "Would Split Help?" Test

For each warning sign, ask:

```
Question 1: Will splitting solve this problem?
✅ Yes: Different teams can deploy independently
❌ No: Split won't help if problem is poor design

Question 2: Can we solve it without splitting?
Example: "Slow tests" might be solved by better test structure
Example: "Large codebase" might need refactoring, not splitting

Question 3: What's the cost of splitting?
- Network latency between services
- Distributed transactions complexity
- Operational overhead (2 services to monitor)
- Data consistency challenges

Question 4: Can we maintain bounded contexts?
✅ Yes: Order and Payment are clearly separate domains
❌ No: Splitting "by layer" (API vs Business Logic) is wrong
```

---

### Step 3: Identify the Split Line

```
Good Split Boundaries:
✅ Domain boundaries (Order vs Payment vs User)
✅ Team boundaries (Team A owns Orders, Team B owns Users)
✅ Data boundaries (OrderDB vs UserDB)
✅ Scaling boundaries (Read-heavy vs Write-heavy)

Bad Split Boundaries:
❌ By layer (API layer vs Business layer vs Data layer)
❌ By CRUD operations (Read service vs Write service)
❌ By technology (Node.js service vs .NET service - unless necessary)
❌ Arbitrary size ("This is too big, split it in half")
```

---

### Step 4: Plan the Migration

```
Phase 1: Identify Boundaries
- Map out current endpoints
- Group by domain/responsibility
- Identify shared dependencies

Phase 2: Extract Domain Logic
- Create separate modules within monolith
- Ensure clean interfaces
- Minimize cross-module calls

Phase 3: Split Data
- Identify which data belongs to which service
- Plan data migration strategy
- Handle foreign key relationships

Phase 4: Deploy as Separate Service
- Extract module to new service
- Set up communication (REST/gRPC/messaging)
- Deploy both services
- Route traffic to new service

Phase 5: Clean Up
- Remove old code from monolith
- Update documentation
- Monitor for issues
```

---

## Real-World Examples

### Example 1: E-commerce Platform Evolution

#### **Year 1: Monolith (Correct Decision)**
```
Single Service: 25 endpoints
Team: 3 developers
Traffic: 1,000 users/day
Deployment: Once per week

Decision: Keep as monolith ✅
Reason: Small team, manageable codebase, simple deployment
```

#### **Year 2: Growing Pains (Warning Signs)**
```
Single Service: 50 endpoints
Team: 12 developers (3 teams)
Traffic: 50,000 users/day
Issues:
- Team A (Catalog) wants to deploy daily
- Team B (Orders) needs monthly regulatory releases
- Team C (Payments) requires PCI-DSS isolation
- Merge conflicts weekly
- Search scaling needs 10x instances, wasting $ on orders

Decision: Time to split 🚩
```

#### **Year 3: Microservices (Strategic Split)**
```
Catalog Service: 15 endpoints
- Team: Catalog Team (4 devs)
- Deploy: Daily
- Scaling: 20 instances (read-heavy)

Order Service: 12 endpoints
- Team: Order Team (4 devs)
- Deploy: Weekly
- Scaling: 5 instances

Payment Service: 8 endpoints
- Team: Payment Team (4 devs)
- Deploy: Monthly (regulatory)
- Scaling: 3 instances
- Isolated for PCI-DSS

Result: ✅
- Teams deploy independently
- Proper scaling per service
- Compliance isolation achieved
- Merge conflicts eliminated
```

---

### Example 2: When NOT to Split

#### **Scenario: Notification Service**
```
Current: 20 endpoints
- Send email (5 endpoints)
- Send SMS (5 endpoints)
- Send push notifications (5 endpoints)
- Notification preferences (5 endpoints)

Proposal: Split into 4 services (EmailService, SMSService, etc.)

Analysis:
❌ Same team owns all
❌ Shared configuration and templates
❌ Shared user preferences
❌ Same deployment schedule
❌ Same scaling needs
❌ Would create network overhead
❌ Would complicate preference management

Decision: Keep as one service ✅
Reason: Strong cohesion, no organizational need to split
```

**Better Approach:** Keep as modules within one service
```
NotificationService:
/modules
  /Email
    - EmailSender.cs
    - EmailTemplates.cs
  /SMS
    - SMSSender.cs
  /Push
    - PushSender.cs
  /Shared
    - PreferenceManager.cs
    - NotificationQueue.cs
```

---

### Example 3: Progressive Split Strategy

#### **Phase 1: Modular Monolith (Start Here)**
```
Single Deployment:
/src
  /Modules
    /Orders
      /Api
      /Domain
      /Data
    /Users
      /Api
      /Domain
      /Data
    /Payments
      /Api
      /Domain
      /Data

Benefits:
✅ Clear boundaries (practice for later split)
✅ Single deployment (simple operations)
✅ No network calls (fast)
✅ Shared database (simple transactions)
```

#### **Phase 2: Extract When Needed**
```
When Order Team grows to 8 people:
→ Extract Orders to separate service

When Payment needs PCI-DSS isolation:
→ Extract Payments to separate service

Users module still small:
→ Keep in original service (now "UserService")
```

---

## Anti-Patterns to Avoid

### ❌ **Anti-Pattern 1: Nano-Services**
```
BAD: Too Granular
UserCreationService: POST /users
UserUpdateService: PUT /users
UserDeletionService: DELETE /users
UserQueryService: GET /users

Problems:
- Network overhead > benefits
- Operational nightmare (4 services to monitor)
- Distributed transactions complex
- No real bounded context

GOOD: Cohesive Service
UserService: All user operations
- Single team
- Single deployment
- Single database
- Clear responsibility
```

### ❌ **Anti-Pattern 2: Horizontal Splits**
```
BAD: Split by Layer
PresentationService: All API endpoints (no logic)
BusinessLogicService: All business rules (calls data layer)
DataAccessService: All database calls

Problems:
- Every request goes through 3 services
- Latency multiplied by 3
- No domain boundaries
- Still a monolith, just distributed
- "Distributed monolith" = worst of both worlds

GOOD: Vertical Slices
OrderService: API + Logic + Data for orders
UserService: API + Logic + Data for users
```

### ❌ **Anti-Pattern 3: Split by CRUD**
```
BAD: Split by Operation Type
ReadService: All GET endpoints
WriteService: All POST/PUT/DELETE endpoints

Problems:
- Breaks domain cohesion
- Data duplication issues
- Not actual microservices
- Doesn't solve real problems

GOOD: Domain-Based
OrderService: All order operations (read + write)
```

### ❌ **Anti-Pattern 4: Premature Splitting**
```
BAD: Day 1 Microservices
"Let's build 15 microservices from the start"

Problems:
- Don't know domain boundaries yet
- Over-engineering
- Operational complexity before value
- Distributed debugging nightmare
- Slower initial development

GOOD: Evolve Architecture
Day 1: Modular monolith (3 modules)
Year 1: Still monolith (6 modules, clear boundaries)
Year 2: Extract 2 high-value services
Year 3: Extract 1 more as needed
```

---

## Post-Split Validation

### After splitting, validate the decision:

#### ✅ **Success Indicators:**

```
Organizational:
□ Teams can deploy independently
□ Release frequency increased
□ Merge conflicts eliminated
□ Clear ownership established

Technical:
□ Failures are isolated
□ Services scale independently
□ Each service has <20 endpoints
□ Response times maintained or improved
□ Operational overhead manageable

Business:
□ Compliance requirements met
□ Different SLAs achievable
□ Cost efficiency improved
□ Time-to-market faster
```

#### 🚩 **Failure Indicators (Consider Merging Back):**

```
□ Network latency became a problem
□ Distributed transactions too complex
□ Teams still coordinating releases
□ Data consistency issues
□ Operational overhead too high (2x monitoring, logging, etc.)
□ Little actual benefit achieved
□ Cost increased significantly
```

---

## Quick Reference Checklist

### Should I Split This Microservice?

**Answer YES if 3+ of these are true:**

- [ ] Multiple teams need to modify it independently
- [ ] Different parts have different scaling needs (10x+ difference)
- [ ] Some features need different SLAs/compliance
- [ ] Release cadence mismatch (some need daily, some monthly)
- [ ] Cascading failures occur regularly
- [ ] Service has 25+ endpoints doing unrelated things
- [ ] Different data ownership/database needs
- [ ] Can clearly articulate 2+ bounded contexts within service
- [ ] Split would enable team independence
- [ ] Current service fails the "one sentence description" test

**Answer NO if 3+ of these are true:**

- [ ] Same team owns everything
- [ ] Same deployment schedule works for all features
- [ ] Same scaling profile
- [ ] Shared data model makes sense
- [ ] Service has <20 endpoints
- [ ] Features are highly cohesive (belong together)
- [ ] Split would create network latency issues
- [ ] Split would require complex distributed transactions
- [ ] Split is just "for microservices sake"
- [ ] Service has clear, single responsibility

---

## Recommended Reading & Resources

### Books:
- "Building Microservices" by Sam Newman
- "Domain-Driven Design" by Eric Evans
- "Monolith to Microservices" by Sam Newman

### Patterns:
- Strangler Fig Pattern (gradual migration)
- Saga Pattern (distributed transactions)
- CQRS (command/query separation)
- Event-Driven Architecture

### Principles:
- Single Responsibility Principle
- Bounded Context (DDD)
- Conway's Law (team structure mirrors architecture)

---

## Summary: The Pragmatic Approach

1. **Start with a Modular Monolith**
   - Clear module boundaries
   - Practice separation
   - Simple operations

2. **Split Only When Needed**
   - Organizational pressure (multiple teams)
   - Technical pressure (scaling, SLAs)
   - Business pressure (compliance, release cadence)

3. **Split at Natural Boundaries**
   - Domain boundaries (DDD)
   - Team boundaries
   - Data boundaries

4. **Validate the Split**
   - Measure benefits
   - Monitor costs
   - Be willing to merge back if wrong

**Remember:** Microservices are a means to an end (team independence, scaling, reliability), not an end in themselves. Split when it solves real problems, not because it's trendy.

---

*Last Updated: November 11, 2025*
