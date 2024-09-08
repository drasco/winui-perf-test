unit Unit1;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, Forms, Controls, Graphics, Dialogs, StdCtrls, ComCtrls;

type

  { TForm1 }

  TForm1 = class(TForm)
    Button1: TButton;
    Label1: TLabel;
    Label2: TLabel;
    ListView1: TListView;
    procedure Button1Click(Sender: TObject);
  private

  public

  end;

var
  Form1: TForm1;

implementation

{$R *.lfm}

{ TForm1 }

procedure TForm1.Button1Click(Sender: TObject);
var
  x: longint;
  dtStart, dtEnd: TDateTime;
begin
     dtStart := Now;
     for x := 0 to 3000 do begin
       ListView1.AddItem(IntToStr( Random(200000000)), Nil);
     end;
     dtEnd := Now;
     Button1.Caption := FormatDateTime('ss.z', dtEnd-dtStart);

     dtStart := Now;
     Sleep(1);
     Sleep(1);
     Sleep(1);
     Sleep(1);
     Sleep(1);
     Sleep(1);
     Sleep(1);
     Sleep(1);
     Sleep(1);
     Sleep(1);
     dtEnd := Now;
     Label1.Caption := FormatDateTime('ss.z', dtEnd-dtStart);

end;


end.

