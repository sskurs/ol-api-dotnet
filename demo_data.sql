-- Insert demo roles
INSERT INTO "Roles" ("Name") VALUES
  ('admin'),
  ('seller'),
  ('consumer')
ON CONFLICT DO NOTHING;

-- Insert demo users
INSERT INTO "Users" ("FirstName", "LastName", "Email", "PasswordHash", "Role", "RoleId", "Status", "Phone", "CreatedAt")
SELECT 'Alice', 'Smith', 'alice@example.com', 'hash1', 'admin', (SELECT "Id" FROM "Roles" WHERE "Name" = 'admin'), 'active', '1234567890', NOW() - INTERVAL '60 days'
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'alice@example.com');
INSERT INTO "Users" ("FirstName", "LastName", "Email", "PasswordHash", "Role", "RoleId", "Status", "Phone", "CreatedAt")
SELECT 'Bob', 'Jones', 'bob@example.com', 'hash2', 'seller', (SELECT "Id" FROM "Roles" WHERE "Name" = 'seller'), 'active', '2345678901', NOW() - INTERVAL '40 days'
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'bob@example.com');
INSERT INTO "Users" ("FirstName", "LastName", "Email", "PasswordHash", "Role", "RoleId", "Status", "Phone", "CreatedAt")
SELECT 'Carol', 'Lee', 'carol@example.com', 'hash3', 'consumer', (SELECT "Id" FROM "Roles" WHERE "Name" = 'consumer'), 'active', '3456789012', NOW() - INTERVAL '20 days'
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'carol@example.com');

-- Insert demo points
INSERT INTO "Points" ("UserId", "Balance", "Earned", "Spent", "LastUpdated")
SELECT "Id", 500, 600, 100, NOW() FROM "Users" WHERE "Email" = 'alice@example.com'
ON CONFLICT DO NOTHING;
INSERT INTO "Points" ("UserId", "Balance", "Earned", "Spent", "LastUpdated")
SELECT "Id", 300, 400, 100, NOW() FROM "Users" WHERE "Email" = 'bob@example.com'
ON CONFLICT DO NOTHING;
INSERT INTO "Points" ("UserId", "Balance", "Earned", "Spent", "LastUpdated")
SELECT "Id", 800, 900, 100, NOW() FROM "Users" WHERE "Email" = 'carol@example.com'
ON CONFLICT DO NOTHING;

-- Insert demo tiers
INSERT INTO "Tiers" ("UserId", "Level", "AssignedAt")
SELECT "Id", 'gold', NOW() - INTERVAL '2 months' FROM "Users" WHERE "Email" = 'alice@example.com'
ON CONFLICT DO NOTHING;
INSERT INTO "Tiers" ("UserId", "Level", "AssignedAt")
SELECT "Id", 'silver', NOW() - INTERVAL '1 month' FROM "Users" WHERE "Email" = 'bob@example.com'
ON CONFLICT DO NOTHING;
INSERT INTO "Tiers" ("UserId", "Level", "AssignedAt")
SELECT "Id", 'bronze', NOW() - INTERVAL '3 months' FROM "Users" WHERE "Email" = 'carol@example.com'
ON CONFLICT DO NOTHING;

-- Insert demo transactions
INSERT INTO "Transactions" ("UserId", "Amount", "Type", "Date")
SELECT "Id", 100, 'purchase', NOW() - INTERVAL '2 days' FROM "Users" WHERE "Email" = 'alice@example.com';
INSERT INTO "Transactions" ("UserId", "Amount", "Type", "Date")
SELECT "Id", 50, 'reward', NOW() - INTERVAL '1 day' FROM "Users" WHERE "Email" = 'alice@example.com';
INSERT INTO "Transactions" ("UserId", "Amount", "Type", "Date")
SELECT "Id", 200, 'purchase', NOW() - INTERVAL '3 days' FROM "Users" WHERE "Email" = 'bob@example.com';
INSERT INTO "Transactions" ("UserId", "Amount", "Type", "Date")
SELECT "Id", 150, 'purchase', NOW() - INTERVAL '5 days' FROM "Users" WHERE "Email" = 'carol@example.com';

-- Optionally, insert demo campaigns
INSERT INTO "Campaigns" ("CampaignId", "Reward", "Name", "Active", "CostInPoints", "Levels", "Segments", "Unlimited", "SingleCoupon", "Coupons")
VALUES
  (gen_random_uuid(), 'Free Coffee', 'Coffee Campaign', true, 100, '[]', '[]', true, true, '[]'),
  (gen_random_uuid(), 'Discount', 'Discount Campaign', true, 200, '[]', '[]', true, true, '[]')
ON CONFLICT DO NOTHING;