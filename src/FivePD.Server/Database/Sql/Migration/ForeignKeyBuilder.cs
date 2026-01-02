namespace FivePD.Server.Database.Sql.Migration;

#pragma warning disable

public class ForeignKeyBuilder
{
    private readonly string _localCol;
    private string _refTable;
    private string _refCol;
    private string _onDelete = "NO ACTION";

    public ForeignKeyBuilder(string localCol) => this._localCol = localCol;

    public ForeignKeyBuilder References(string table, string column)
    {
        this._refTable = table;
        this._refCol = column;
        return this;
    }

    public ForeignKeyBuilder OnDeleteCascade() { this._onDelete = "CASCADE"; return this; }
    public ForeignKeyBuilder OnDeleteSetNull() { this._onDelete = "SET NULL"; return this; }
    public ForeignKeyBuilder OnDeleteNoAction() { this._onDelete = "NO ACTION"; return this; }

    public string Build() => 
        $"FOREIGN KEY(`{this._localCol}`) REFERENCES `{this._refTable}`(`{this._refCol}`) ON DELETE {this._onDelete}";
}