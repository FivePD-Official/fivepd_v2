fx_version 'cerulean'
game 'gta5'

author 'FivePD'
version '2.0.0'

ui_page 'http://localhost:4200'
client_script 'Libraries/FivePD.Client/FivePD.Client.net.dll'
server_script 'Libraries/FivePD.Server/FivePD.Server.net.dll'

files {
    'Libraries/FivePD.Client/**/*',
    'Libraries/FivePD.Nui/**/*',
    'Localization/**/*',
	'Config/**/*'
}