﻿Imports System.Data.SqlClient
Imports System.Runtime.InteropServices.ComTypes
Imports MySql.Data.MySqlClient
Imports Mysqlx
Public Class TransportAdminDLReq
    Private uid As Integer
    Private Accept_click As Integer = 0
    Private Reject_click As Integer = 0
    Private row1 As Integer = 0
    Private vType As String
    Private vTypeId As Integer
    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        ' Check if the clicked cell is in the "Column7" column and not a header cell
        If e.ColumnIndex = DataGridView1.Columns("Column7").Index AndAlso e.RowIndex >= 0 Then
            ' Change this to DB logic later
            Accept_click = 1
            row1 = e.RowIndex
            Dim cellValue As Object = DataGridView1.Rows(row1).Cells(0).Value
            Integer.TryParse(cellValue.ToString(), uid)
            vTypeId = DataGridView1.Rows(row1).Cells(3).Value
            vType = TransportGlobals.GetVehicleType(vTypeId)
            LoadandBindDataGridView()

            ' Check if the clicked cell is in the "Column8" column and not a header cell
        ElseIf e.ColumnIndex = DataGridView1.Columns("Column8").Index AndAlso e.RowIndex >= 0 Then
            ' Perform the action for the "Column8" column
            Reject_click = 1
            row1 = e.RowIndex
            Dim cellValue As Object = DataGridView1.Rows(row1).Cells(0).Value
            Integer.TryParse(cellValue.ToString(), uid)
            vTypeId = DataGridView1.Rows(row1).Cells(3).Value
            vType = TransportGlobals.GetVehicleType(vTypeId)
            LoadandBindDataGridView()

        End If

    End Sub

    Private Sub LoadandBindDataGridView()
        'Get connection from globals
        Dim Con = Globals.GetDBConnection()
        Dim reader As MySqlDataReader
        Dim cmd As MySqlCommand

        Try
            Con.Open()

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Using command As New MySqlCommand("update dl_entries set test_status = @c,issued_on = @d, valid_till = @e where uid = @a and vehicle_type = @b", Con)

            command.Parameters.AddWithValue("@a", uid)


            command.Parameters.AddWithValue("@b", vTypeId)
            If Accept_click = 1 Then

                command.Parameters.AddWithValue("@c", "pass")
                command.Parameters.AddWithValue("@d", DateTime.Today)
                command.Parameters.AddWithValue("@e", DateTime.Today.AddYears(10))
                Accept_click = 0
                ' Execute the command (Update statement)
                command.ExecuteNonQuery()
                Globals.SendNotifications(4, uid, "Driving License Approved", "Your Request for Driving License for vehicle Type " & vType & " is approved you can view your Driving License in the Driving License Request Page")
            ElseIf Reject_click = 1 Then

                command.Parameters.AddWithValue("@c", "fail")
                command.Parameters.AddWithValue("@d", DBNull.Value)
                command.Parameters.AddWithValue("@e", DBNull.Value)
                Reject_click = 0
                ' Execute the command (Update statement)
                command.ExecuteNonQuery()
                Globals.SendNotifications(4, uid, "Driving License Request Rejected", "Your Request for Driving License for vehicle Type " & vType & " is rejected you can view your Driving License in the Driving License Request Page")
            End If

        End Using

        cmd = New MySqlCommand("SELECT uid, vehicle_type, req_type, fee_paid, name, age FROM dl_entries JOIN users ON dl_entries.uid = users.user_id where test_status is NULL ", Con)
        reader = cmd.ExecuteReader
        ' Create a DataTable to store the data
        Dim dataTable As New DataTable()

        'Fill the DataTable with data from the SQL table
        dataTable.Load(reader)
        reader.Close()
        Con.Close()

        'IMP: Specify the Column Mappings from DataGridView to SQL Table
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.Columns(0).DataPropertyName = "uid"
        DataGridView1.Columns(1).DataPropertyName = "name"
        DataGridView1.Columns(2).DataPropertyName = "age"
        DataGridView1.Columns(3).DataPropertyName = "vehicle_type"
        DataGridView1.Columns(4).DataPropertyName = "req_type"
        DataGridView1.Columns(5).DataPropertyName = "fee_paid"

        ' Bind the data to DataGridView
        DataGridView1.DataSource = dataTable
    End Sub


    Private Sub TransportationInnerScreen_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        LoadandBindDataGridView()
    End Sub

End Class