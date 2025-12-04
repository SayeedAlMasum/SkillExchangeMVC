using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillExchangeMVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBkashFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BkashMerchantInvoiceNumber",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "BkashPayerReference",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "BkashPaymentId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "BkashTransactionId",
                table: "Payment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BkashMerchantInvoiceNumber",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BkashPayerReference",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BkashPaymentId",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BkashTransactionId",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
