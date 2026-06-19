using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClimbTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "difficulty_levels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    grade_range = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    max_days_week = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_difficulty_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "energy_systems",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    duration_range = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_energy_systems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "phases",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plan_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    owner_user_id = table.Column<long>(type: "bigint", nullable: true),
                    phase_id = table.Column<int>(type: "integer", nullable: true),
                    difficulty_level_id = table.Column<int>(type: "integer", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_custom_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    load_level = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_custom_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    difficulty_level_id = table.Column<int>(type: "integer", nullable: false),
                    preferred_locale = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_difficulty_levels_difficulty_level_id",
                        column: x => x.difficulty_level_id,
                        principalTable: "difficulty_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "session_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    load_level = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    energy_system_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_session_types_energy_systems_energy_system_id",
                        column: x => x.energy_system_id,
                        principalTable: "energy_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_plans",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    training_type_id = table.Column<int>(type: "integer", nullable: false),
                    difficulty_level_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_plans_difficulty_levels_difficulty_level_id",
                        column: x => x.difficulty_level_id,
                        principalTable: "difficulty_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_plans_training_types_training_type_id",
                        column: x => x.training_type_id,
                        principalTable: "training_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_custom_session_blocks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_custom_session_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_custom_session_blocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_custom_session_blocks_user_custom_sessions_user_custom~",
                        column: x => x.user_custom_session_id,
                        principalTable: "user_custom_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_template_days",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plan_template_id = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    session_type_id = table.Column<int>(type: "integer", nullable: true),
                    is_rest = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_template_days", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_template_days_plan_templates_plan_template_id",
                        column: x => x.plan_template_id,
                        principalTable: "plan_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plan_template_days_session_types_session_type_id",
                        column: x => x.session_type_id,
                        principalTable: "session_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "session_blocks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_type_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_blocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_session_blocks_session_types_session_type_id",
                        column: x => x.session_type_id,
                        principalTable: "session_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_session_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    user_plan_week_id = table.Column<long>(type: "bigint", nullable: true),
                    session_type_id = table.Column<int>(type: "integer", nullable: false),
                    log_date = table.Column<DateOnly>(type: "date", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    is_done = table.Column<bool>(type: "boolean", nullable: false),
                    rpe = table.Column<int>(type: "integer", nullable: true),
                    duration_min = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_session_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_session_logs_session_types_session_type_id",
                        column: x => x.session_type_id,
                        principalTable: "session_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_plan_weeks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_plan_id = table.Column<long>(type: "bigint", nullable: false),
                    week_number = table.Column<int>(type: "integer", nullable: false),
                    phase_id = table.Column<int>(type: "integer", nullable: false),
                    plan_template_id = table.Column<int>(type: "integer", nullable: true),
                    is_deload = table.Column<bool>(type: "boolean", nullable: false),
                    progress_pct = table.Column<double>(type: "double precision", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_plan_weeks", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_plan_weeks_phases_phase_id",
                        column: x => x.phase_id,
                        principalTable: "phases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_plan_weeks_user_plans_user_plan_id",
                        column: x => x.user_plan_id,
                        principalTable: "user_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_block_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_block_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_block_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_session_block_items_session_blocks_session_block_id",
                        column: x => x.session_block_id,
                        principalTable: "session_blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_log_metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_log_id = table.Column<long>(type: "bigint", nullable: false),
                    metric_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    metric_value = table.Column<string>(type: "text", nullable: false),
                    metric_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_log_metrics", x => x.id);
                    table.ForeignKey(
                        name: "FK_session_log_metrics_user_session_logs_session_log_id",
                        column: x => x.session_log_id,
                        principalTable: "user_session_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_difficulty_levels_code",
                table: "difficulty_levels",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_energy_systems_code",
                table: "energy_systems",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phases_code",
                table: "phases",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_template_days_plan_template_id_day_of_week",
                table: "plan_template_days",
                columns: new[] { "plan_template_id", "day_of_week" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_template_days_session_type_id",
                table: "plan_template_days",
                column: "session_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_block_items_session_block_id_sort_order",
                table: "session_block_items",
                columns: new[] { "session_block_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_blocks_session_type_id_sort_order",
                table: "session_blocks",
                columns: new[] { "session_type_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_log_metrics_session_log_id_metric_key",
                table: "session_log_metrics",
                columns: new[] { "session_log_id", "metric_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_types_code",
                table: "session_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_types_energy_system_id",
                table: "session_types",
                column: "energy_system_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_types_code",
                table: "training_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_custom_session_blocks_user_custom_session_id_sort_order",
                table: "user_custom_session_blocks",
                columns: new[] { "user_custom_session_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_weeks_phase_id",
                table: "user_plan_weeks",
                column: "phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_weeks_user_plan_id_week_number",
                table: "user_plan_weeks",
                columns: new[] { "user_plan_id", "week_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_plans_difficulty_level_id",
                table: "user_plans",
                column: "difficulty_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plans_training_type_id",
                table: "user_plans",
                column: "training_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plans_user_id_is_active",
                table: "user_plans",
                columns: new[] { "user_id", "is_active" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_user_session_logs_session_type_id",
                table: "user_session_logs",
                column: "session_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_session_logs_user_id_log_date",
                table: "user_session_logs",
                columns: new[] { "user_id", "log_date" });

            migrationBuilder.CreateIndex(
                name: "IX_users_difficulty_level_id",
                table: "users",
                column: "difficulty_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plan_template_days");

            migrationBuilder.DropTable(
                name: "session_block_items");

            migrationBuilder.DropTable(
                name: "session_log_metrics");

            migrationBuilder.DropTable(
                name: "user_custom_session_blocks");

            migrationBuilder.DropTable(
                name: "user_plan_weeks");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "plan_templates");

            migrationBuilder.DropTable(
                name: "session_blocks");

            migrationBuilder.DropTable(
                name: "user_session_logs");

            migrationBuilder.DropTable(
                name: "user_custom_sessions");

            migrationBuilder.DropTable(
                name: "phases");

            migrationBuilder.DropTable(
                name: "user_plans");

            migrationBuilder.DropTable(
                name: "session_types");

            migrationBuilder.DropTable(
                name: "difficulty_levels");

            migrationBuilder.DropTable(
                name: "training_types");

            migrationBuilder.DropTable(
                name: "energy_systems");
        }
    }
}
