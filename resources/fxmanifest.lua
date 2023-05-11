-- Editing any of the lines below is not supported and may cause unexpected behavior.

fx_version 'cerulean'
game 'gta5'

author 'FivePD'
version '2.0.0'

ui_page 'http://localhost:3000'
client_script 'Libraries/FivePD.Gamemode.Client/FivePD.Gamemode.Client.net.dll'
server_script 'Libraries/FivePD.Gamemode.Server/FivePD.Gamemode.Server.net.dll'

files {
    'Libraries/FivePD.Gamemode.Client/**/*',
    'Libraries/FivePD.Nui/**/*',
    'Localization/**/*',
	'Config/**/*'
}