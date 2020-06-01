table 50051 Imported_Kontakt
{
    DataClassification = ToBeClassified;

    fields
    {
        field(50001; Imported_KontaktNr; Code[10]) { }
        field(50002; Imported_BrugerKontaktNr; Code[10]) { }
        field(50003; Imported_KundeNr; Code[10]) { }
        field(50004; Imported_LeverandoerNr; Code[20]) { }
        field(50005; Imported_Navn; Text[50]) { }
        field(50006; Imported_Telefon; Text[50]) { }
        field(50007; Imported_Email; Text[80]) { }
    }
    keys
    {
        key(PK; Imported_KontaktNr)
        {
            Clustered = true;
        }
    }
}

tableextension 50051 ExtContact extends Contact
{
    fields
    {
        field(50000; ImportNote; Text[1024]) { }
    }

    procedure ImportPhone(importedValue: Text[1024])
    var
        maxLen: Integer;
        delimiterPos: Integer;
    begin
        maxLen := MaxStrLen("Phone No.");
        if StrLen(importedValue) <= maxLen then begin
            "Phone No." := importedValue;
            exit;
        end;

        ImportNote += '\' + FieldCaption("Phone No.") + ': ' + importedValue;
        if importedValue.Contains('/') then begin
            delimiterPos := importedValue.IndexOf('/');
            if delimiterPos < maxLen then begin
                "Phone No." := CopyStr(importedValue, 1, delimiterPos - 1);
                exit;
            end;
        end;

        "Phone No." := CopyStr(importedValue, 1, maxLen);
    end;

    procedure ImportEmail(importedValue: Text[1024])
    var
        maxLen: Integer;
        delimiterPos: Integer;
    begin
        maxLen := MaxStrLen("E-Mail");
        if StrLen(importedValue) <= maxLen then begin
            "E-Mail" := importedValue;
            exit;
        end;

        ImportNote += '\' + FieldCaption("E-Mail") + ': ' + importedValue;
        if importedValue.Contains('/') then begin
            delimiterPos := importedValue.IndexOf('/');
            if delimiterPos < maxLen then begin
                "E-Mail" := CopyStr(importedValue, 1, delimiterPos - 1);
                exit;
            end;
        end;

        "E-Mail" := CopyStr(importedValue, 1, maxLen);
    end;
}

codeunit 50057 "Post-Kontakt"
{
    TableNo = "Data Exch.";
    trigger OnRun();
    var
        mMarketSetup: Record "Marketing Setup";
    begin
        mMarketSetup.GET;
        mMarketSetup.TestField("Bus. Rel. Code for Customers");

        CreateContact();
    end;


    local procedure CreateContact();
    var
        mImCont_C: Record Imported_Kontakt;
        mImCont_V: Record Imported_Kontakt;
    begin
        with mImCont_C do begin
            SetFilter(Imported_KundeNr, '<>%1', '');
            if mImCont_C.FindSet() then
                repeat
                    CreateContactCustomer(mImCont_C);
                until mImCont_C.Next() = 0;
        end;

        with mImCont_V do begin
            SetFilter(Imported_LeverandoerNr, '<>%1', '');
            if FindSet() then
                repeat
                    CreateContactVendor(mImCont_V);
                until Next() = 0;
        end;
    end;

    local procedure CreateContactCustomer(ImCont: Record Imported_Kontakt)
    var
        mContBusRel: Record "Contact Business Relation";
        mCust: Record Customer;
        UpdateContFromCust: Codeunit "CustCont-Update";
    begin
        if mCust.Get(ImCont.Imported_KundeNr) then begin
            mContBusRel.SetRange("Link to Table", mContBusRel."Link to Table"::Customer);
            mContBusRel.SetRange("No.", ImCont.Imported_KundeNr);
            if not mContBusRel.FindFirst() then begin
                UpdateContFromCust.InsertNewContact(mCust, FALSE);
                mContBusRel.FindFirst();
            end;

            InsertNewContactPerson(ImCont.Imported_KontaktNr, ImCont.Imported_KundeNr, ImCont, mContBusRel."Contact No.");

            if ImCont.Imported_BrugerKontaktNr = '1' then begin
                mCust.VALIDATE("Primary Contact No.", ImCont.Imported_KontaktNr);
                mCust.MODIFY;
            end;
        end;
    end;

    local procedure CreateContactVendor(ImCont: Record Imported_Kontakt)
    var
        mContBusRel: Record "Contact Business Relation";
        mVend: Record Vendor;
        UpdateContFromVend: Codeunit "VendCont-Update";
    begin
        if mVend.Get(ImCont.Imported_LeverandoerNr) then begin
            mContBusRel.SetRange("Link to Table", mContBusRel."Link to Table"::Vendor);
            mContBusRel.SetRange("No.", ImCont.Imported_LeverandoerNr);
            if not mContBusRel.FindFirst() then begin
                UpdateContFromVend.InsertNewContact(mVend, FALSE);
                mContBusRel.FindFirst();
            end;

            InsertNewContactPerson(ImCont.Imported_KontaktNr, ImCont.Imported_LeverandoerNr, ImCont, mContBusRel."Contact No.");

            if ImCont.Imported_BrugerKontaktNr = '1' then begin
                mVend.VALIDATE("Primary Contact No.", ImCont.Imported_KontaktNr);
                mVend.MODIFY;
            end;
        end;
    end;

    local procedure InsertNewContactPerson(ContactNo: Code[20]; CustNo: Code[20]; ImCont: Record Imported_Kontakt; BusRelContactNo: Code[20]);
    var
        mContact: Record Contact;
        mContact2: Record Contact;
    begin
        IF mContact.GET(BusRelContactNo) THEN
            WITH mContact2 DO BEGIN
                INIT;
                "No." := ContactNo;
                VALIDATE(Type, Type::Person);
                INSERT(TRUE);
                "Company No." := mContact."No.";

                Name := ImCont.Imported_Navn;
                ImportPhone(ImCont.Imported_Telefon);
                ImportEmail(ImCont.Imported_Email);

                InheritCompanyToPersonData(mContact);
                MODIFY(TRUE);
            END
    end;

    local procedure updExtension(RecVar: Variant; fldId: Integer; txt: Variant);
    var
        recRef: RecordRef;
        fldRefTo: FieldRef;
    begin
        recRef.OPEN(DATABASE::Contact);
        IF recRef.GET(RecVar) THEN BEGIN
            fldRefTo := recRef.FIELD(fldId);
            fldRefTo.VALUE(txt);
            recRef.MODIFY;
        END;
    end;

}