BEGIN TRANSACTION;
CREATE TABLE "Departments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Departments" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);

CREATE TABLE "Staff" (
    "UserId" TEXT NOT NULL CONSTRAINT "PK_Staff" PRIMARY KEY,
    "DepartmentId" INTEGER NOT NULL,
    "EnabledAt" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    "DeletedAt" TEXT NOT NULL,
    CONSTRAINT "FK_Staff_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Staff_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Staff_DepartmentId" ON "Staff" ("DepartmentId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251206173636_AddStaffAndDepartment', '10.0.0');

COMMIT;

