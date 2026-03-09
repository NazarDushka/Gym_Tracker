using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddBodyMeasurementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "BodyFatPercentage",
                table: "BodyMeasurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Calves",
                table: "BodyMeasurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Forearms",
                table: "BodyMeasurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Height",
                table: "BodyMeasurements",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Hips",
                table: "BodyMeasurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Legs",
                table: "BodyMeasurements",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyFatPercentage",
                table: "BodyMeasurements");

            migrationBuilder.DropColumn(
                name: "Calves",
                table: "BodyMeasurements");

            migrationBuilder.DropColumn(
                name: "Forearms",
                table: "BodyMeasurements");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "BodyMeasurements");

            migrationBuilder.DropColumn(
                name: "Hips",
                table: "BodyMeasurements");

            migrationBuilder.DropColumn(
                name: "Legs",
                table: "BodyMeasurements");
        }
    }
}
