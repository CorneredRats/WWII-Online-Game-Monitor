/* =============================================================================
 * Xiperware Wiretap API                            Copyright (c) 2013 Xiperware
 * http://begm.sourceforge.net/                              xiperware@gmail.com
 * 
 * This file is part of the Xiperware Wiretap API library for WW2 Online.
 * 
 * This library is free software; you can redistribute it and/or modify it under
 * the terms of the GNU Lesser General Public License version 2.1 as published
 * by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Xml;
using Xiperware.WiretapAPI.Properties;

namespace Xiperware.WiretapAPI
{
  /// <summary>
  /// A generic class for storing static data used by WW2 Online.
  /// </summary>
  public static class Data
  {
    /// <summary>
    /// Gets data for the average altitude of each chokepoint. 
    /// </summary>
    /// <remarks>
    /// Data extracted from http://www.battlegroundtools.com/documents/Altitudes-128.pdf
    /// by FOGABAN, used with permission.
    /// </remarks>
    /// <param name="minLength">Minimum length of array.</param>
    /// <returns>An array of altitude heights in meters, indexed by cp id.</returns>
    public static int[] GetAltitudes( int minLength )
    {
      int len = 794;
      if( minLength > len )
        len = minLength;

      int[] alt = new int[len];


      // generated Wed Jun 11 09:28:51 2008 by altpdf2array.pl
      alt[1]   = 23;    // Chichester
      alt[2]   = 2;     // Gosport
      alt[23]  = 202;   // Mendig
      alt[24]  = 484;   // Hahn
      alt[25]  = 59;    // Oplade
      alt[26]  = 52;    // Langenfeld
      alt[28]  = 50;    // Köln
      alt[32]  = 38;    // Düsseldorf
      alt[36]  = 40;    // Ratingen
      alt[39]  = 46;    // Dormagen
      alt[41]  = 41;    // Neuss
      alt[42]  = 64;    // Brühl
      alt[43]  = 93;    // Kerpen
      alt[44]  = 165;   // Euskirchen
      alt[45]  = 532;   // Nettersheim
      alt[46]  = 452;   // Hillesheim
      alt[47]  = 41;    // Kaarst
      alt[48]  = 35;    // Krefeld
      alt[50]  = 37;    // Kempen
      alt[51]  = 143;   // Grevenbroich
      alt[52]  = 70;    // Monchen Gladbach
      alt[53]  = 128;   // Duren
      alt[54]  = 101;   // Bergheim
      alt[55]  = 160;   // Wollersheim
      alt[56]  = 518;   // Schleiden
      alt[57]  = 528;   // Densborn
      alt[58]  = 485;   // Prum
      alt[59]  = 497;   // Gerolstein
      alt[60]  = 305;   // Bitburg
      alt[61]  = 185;   // Echternach
      alt[62]  = 135;   // Grevenmacher
      alt[63]  = 52;    // Nettetal
      alt[66]  = 40;    // Venlo
      alt[67]  = 98;    // Erkelenz
      alt[68]  = 65;    // Neiderkruchten
      alt[69]  = 88;    // Julich
      alt[70]  = 395;   // Hürtgenwald
      alt[71]  = 600;   // Hallschlag
      alt[72]  = 605;   // Stadtkyll
      alt[73]  = 525;   // Monschau
      alt[74]  = 335;   // Vianden
      alt[75]  = 140;   // Remich
      alt[77]  = 37;    // Thionville
      alt[81]  = 166;   // Metz
      alt[84]  = 32;    // Griendtsveen
      alt[86]  = 40;    // Panningen
      alt[87]  = 98;    // Sittard
      alt[88]  = 40;    // Heinsberg
      alt[90]  = 30;    // Roermond
      alt[92]  = 30;    // Roermond West
      alt[93]  = 163;   // Aachen
      alt[94]  = 137;   // Heerlen
      alt[95]  = 87;    // Geilenkirchen
      alt[96]  = 290;   // Eupen
      alt[97]  = 453;   // Stavelot
      alt[98]  = 486;   // Malmedy
      alt[99]  = 482;   // Gouvy
      alt[100] = 462;   // St.Vith
      alt[101] = 454;   // Wiltz
      alt[102] = 495;   // Clervaux
      alt[103] = 355;   // Tuntange
      alt[104] = 221;   // Mersch
      alt[105] = 204;   // Ettelbruck
      alt[106] = 457;   // Heiderscheid
      alt[107] = 364;   // Esch
      alt[108] = 274;   // Luxembourg
      alt[109] = 305;   // Fontoy
      alt[111] = 210;   // Jarny
      alt[112] = 20;    // Helmond
      alt[113] = 33;    // Weert
      alt[114] = 28;    // Eind
      alt[115] = 35;    // Maaseik
      alt[122] = 59;    // Maastricht
      alt[124] = 88;    // Elsloo
      alt[127] = 268;   // Verviers
      alt[128] = 7;     // Vise
      alt[129] = 297;   // Chevron
      alt[130] = 309;   // Spa
      alt[131] = 213;   // Sprimont
      alt[132] = 381;   // Houffalize
      alt[133] = 482;   // Manhay
      alt[134] = 511;   // Bastogne
      alt[135] = 472;   // Flamierge
      alt[136] = 384;   // Habay
      alt[137] = 448;   // Martelange
      alt[138] = 442;   // Cobreville
      alt[139] = 316;   // Longwy
      alt[140] = 389;   // Arlon
      alt[141] = 314;   // Piennes
      alt[142] = 261;   // Spincourt
      alt[143] = 311;   // Longuyon
      alt[144] = 215;   // Etain
      alt[145] = 18;    // Eindhoven
      alt[146] = 14;    // Veghel
      alt[147] = 38;    // Achel
      alt[148] = 26;    // Valkenswaard
      alt[149] = 73;    // Helchteren
      alt[150] = 67;    // Peer
      alt[151] = 58;    // Bilzen
      alt[152] = 91;    // Tongeren
      alt[155] = 49;    // Hasselt
      alt[156] = 78;    // Genk
      alt[157] = 84;    // Ramet
      alt[163] = 81;    // Liege
      alt[164] = 538;   // Hamoir
      alt[165] = 280;   // Mean
      alt[166] = 220;   // Nandrin
      alt[167] = 360;   // LaRoche
      alt[168] = 338;   // Marche
      alt[169] = 215;   // Hampteau
      alt[170] = 466;   // St.Hubert
      alt[171] = 410;   // Champlon
      alt[172] = 450;   // Neufchateau
      alt[173] = 469;   // Libramont
      alt[174] = 198;   // Montmedy
      alt[175] = 238;   // Virton
      alt[176] = 235;   // Orval
      alt[177] = 221;   // Damvillers
      alt[178] = 133;   // Verdun
      alt[180] = 9;     // Waalwijk
      alt[181] = 9;     // S-Hertogenbosch
      alt[182] = 14;    // Tilburg
      alt[183] = 9;     // Boxtel
      alt[184] = 50;    // Lommel
      alt[185] = 27;    // Arendonk
      alt[186] = 32;    // Eersel
      alt[187] = 31;    // Paal
      alt[191] = 53;    // Leopoldsburg
      alt[192] = 56;    // Mol
      alt[193] = 62;    // St.Truiden
      alt[194] = 194;   // Bouillet
      alt[195] = 140;   // Hannut
      alt[196] = 120;   // Waremme
      alt[198] = 85;    // Andenne
      alt[199] = 264;   // Havelange
      alt[201] = 84;    // Huy
      alt[202] = 273;   // Rochefort
      alt[203] = 255;   // Ciney
      alt[204] = 418;   // Libin
      alt[205] = 250;   // Wellin
      alt[206] = 308;   // Herbeumont
      alt[207] = 308;   // Bouillon
      alt[208] = 428;   // Bertrix
      alt[209] = 133;   // Mouzon
      alt[210] = 175;   // Carignan
      alt[211] = 341;   // Florenville
      alt[213] = 136;   // Consenvoye
      alt[216] = 136;   // Dun
      alt[218] = 172;   // Stenay
      alt[219] = 293;   // Montfaucon
      alt[220] = 9;     // Oosterhout
      alt[221] = 26;    // Baarle-Hertog
      alt[222] = 17;    // Gilze
      alt[223] = 21;    // Turnhout
      alt[225] = 20;    // Geel
      alt[226] = 44;    // Tienen
      alt[227] = 55;    // Aarschot
      alt[228] = 30;    // Diest
      alt[229] = 162;   // Eghezee
      alt[230] = 108;   // Jodoigne
      alt[231] = 90;    // Profondeville
      alt[235] = 90;    // Namur
      alt[241] = 108;   // Anhee
      alt[244] = 99;    // Dinant
      alt[245] = 236;   // Feschaux
      alt[247] = 104;   // Givet
      alt[249] = 133;   // Hastiere
      alt[250] = 209;   // Spontin
      alt[251] = 87;    // Bievre
      alt[252] = 376;   // Gedinne
      alt[253] = 229;   // Beauraing
      alt[258] = 134;   // Sedan
      alt[259] = 148;   // Grandpre
      alt[260] = 215;   // Buzancy
      alt[261] = 15;    // Zundert
      alt[262] = 12;    // Etten-Leur
      alt[263] = 6;     // Breda
      alt[264] = 20;    // Oostmalle
      alt[266] = 8;     // Schilde
      alt[267] = 20;    // Wuustwezel
      alt[268] = 20;    // Hoogstraten
      alt[269] = 3;     // Lier
      alt[270] = 20;    // Grobbendonk
      alt[272] = 21;    // Leuven
      alt[273] = 168;   // Gembloux
      alt[274] = 84;    // Wavre
      alt[276] = 105;   // Sambreville
      alt[279] = 258;   // Flavion
      alt[280] = 259;   // Mettet
      alt[282] = 111;   // Haybes
      alt[284] = 172;   // Revin
      alt[286] = 177;   // Vireaux
      alt[290] = 133;   // Charleville
      alt[296] = 131;   // Montherme
      alt[299] = 171;   // LeChesne
      alt[300] = 220;   // Launois
      alt[301] = 155;   // Mazagran
      alt[304] = 99;    // Vouziers
      alt[307] = 87;    // Attigny
      alt[310] = 137;   // Sechault
      alt[311] = 2;     // Willemstad
      alt[312] = 2;     // Bergen Op Zoom
      alt[313] = 8;     // Roosendaal
      alt[314] = 2;     // Steenbergen
      alt[320] = 2;     // Antwerp
      alt[321] = 5;     // Zandvliet
      alt[322] = 23;    // Kalmthout
      alt[323] = 3;     // Boom
      alt[324] = 3;     // Mechelen
      alt[327] = 55;    // Brussels
      alt[328] = 127;   // Nivelles
      alt[329] = 117;   // Waterloo
      alt[330] = 107;   // Charleroi
      alt[332] = 250;   // Cerfontaine
      alt[333] = 232;   // Philipville
      alt[334] = 229;   // Walcourt
      alt[335] = 223;   // Somzee
      alt[337] = 247;   // Chimay
      alt[338] = 206;   // Couvin
      alt[339] = 160;   // Mariemburg
      alt[340] = 245;   // Antheny
      alt[341] = 206;   // Chilly
      alt[342] = 238;   // Liart
      alt[343] = 344;   // Rocroi
      alt[347] = 72;    // Rethel
      alt[348] = 166;   // Signy
      alt[349] = 98;    // Juniville
      alt[350] = 2;     // Hellevoetsluis
      alt[351] = 2;     // Krabbendijke
      alt[352] = 2;     // Gorisboek
      alt[353] = 2;     // Hulst
      alt[354] = 5;     // Stekene
      alt[355] = 2;     // Walsoorden
      alt[359] = 2;     // Dendermonde
      alt[360] = 2;     // Temse
      alt[362] = 15;    // St.Niklaas
      alt[363] = 20;    // Ninove
      alt[364] = 9;     // Aalst
      alt[365] = 101;   // Soignies
      alt[366] = 75;    // Enghien
      alt[367] = 44;    // Halle
      alt[368] = 123;   // Binche
      alt[369] = 130;   // Louviere
      alt[370] = 186;   // Beaumont
      alt[372] = 137;   // Merbes
      alt[373] = 239;   // Trelon
      alt[374] = 183;   // Rozoy
      alt[375] = 218;   // Aubenton
      alt[376] = 191;   // Hirson
      alt[377] = 122;   // Chaumont
      alt[378] = 123;   // Dizy
      alt[379] = 67;    // Gomont
      alt[380] = 151;   // Lislet
      alt[381] = 84;    // Warmeriville
      alt[382] = 64;    // Neufchatel
      alt[387] = 85;    // Reims
      alt[389] = 2;     // Gravenpolder
      alt[390] = 2;     // Zelzate
      alt[392] = 133;   // Terneuzen
      alt[394] = 3;     // Wetteren
      alt[396] = 3;     // Lokeren
      alt[397] = 41;    // Brakel
      alt[398] = 64;    // Zottegem
      alt[399] = 41;    // Ath
      alt[400] = 75;    // Geraardsbergen
      alt[401] = 37;    // Thulin
      alt[402] = 43;    // Mons
      alt[403] = 75;    // Jurbise
      alt[406] = 145;   // Berlaimont
      alt[408] = 143;   // Maubeuge
      alt[409] = 149;   // Bavay
      alt[410] = 221;   // LaCapelle
      alt[411] = 197;   // Nouvion
      alt[412] = 157;   // Avesnes
      alt[413] = 112;   // Marle
      alt[414] = 172;   // Vervins
      alt[415] = 85;    // Sissonne
      alt[418] = 64;    // Berry-Au-Bac
      alt[421] = 2;     // Vlissingen
      alt[422] = 3;     // Ijzendijke
      alt[423] = 2;     // Breskens
      alt[428] = 6;     // Gent
      alt[434] = 8;     // Eeklo
      alt[437] = 14;    // Oudenaarde
      alt[438] = 12;    // Deinze
      alt[439] = 58;    // Leuze
      alt[440] = 43;    // Ronse
      alt[442] = 21;    // Valenciennes
      alt[448] = 20;    // Conde
      alt[449] = 91;    // Solesmes
      alt[450] = 134;   // LeQuesnoy
      alt[452] = 139;   // Catillon
      alt[455] = 145;   // Landrecies
      alt[456] = 145;   // Sains-Richaumont
      alt[457] = 98;    // Neuvillette
      alt[458] = 114;   // Guise
      alt[459] = 70;    // Laon
      alt[460] = 5;     // Brugge
      alt[461] = 9;     // Maldegem
      alt[462] = 2;     // Zeebrugge
      alt[463] = 3;     // Knokke
      alt[464] = 41;    // Tielt
      alt[465] = 15;    // Aalter
      alt[467] = 14;    // Avelgem
      alt[468] = 24;    // Kortrijk
      alt[469] = 15;    // Tournai
      alt[473] = 37;    // Orchies
      alt[474] = 98;    // Cambrai
      alt[477] = 38;    // Bouchain
      alt[478] = 136;   // Bohain
      alt[479] = 101;   // Le Catelet
      alt[480] = 122;   // Caudry
      alt[489] = 81;    // St.Quentin
      alt[492] = 52;    // La Fere
      alt[494] = 20;    // Torhout
      alt[495] = 8;     // Jabbeke
      alt[496] = 23;    // Menen
      alt[497] = 23;    // Roulers
      alt[498] = 38;    // Lille
      alt[499] = 34;    // Roubaix
      alt[500] = 34;    // Tourcoing
      alt[501] = 123;   // Douai
      alt[502] = 29;    // Seclin
      alt[505] = 38;    // Fechain
      alt[506] = 69;    // Marquion
      alt[507] = 55;    // Vitry-En-Artois
      alt[508] = 58;    // Peronne
      alt[509] = 95;    // Roisel
      alt[510] = 125;   // Bertincourt
      alt[513] = 5;     // Oostende
      alt[514] = 14;    // Diksmuide
      alt[515] = 32;    // Ieper
      alt[516] = 30;    // Poperinge
      alt[517] = 18;    // Armentieres
      alt[518] = 21;    // Bailleul
      alt[519] = 44;    // Lens
      alt[520] = 26;    // La Bassee
      alt[521] = 67;    // Arras
      alt[529] = 130;   // Bapaume
      alt[530] = 85;    // Chuignolles
      alt[532] = 3;     // Veurne
      alt[533] = 114;   // Cassel
      alt[534] = 3;     // Bergues
      alt[535] = 29;    // Lillers
      alt[536] = 30;    // Hazebrouck
      alt[537] = 128;   // Aubigny
      alt[538] = 166;   // Bruay
      alt[539] = 26;    // Bethune
      alt[540] = 172;   // Saulty
      alt[541] = 98;    // Corbie
      alt[542] = 76;    // Albert
      alt[543] = 104;   // Villers-Bretonneux
      alt[546] = 91;    // Clermont
      alt[547] = 5;     // Dunkerque
      alt[548] = 11;    // Watten
      alt[549] = 9;     // St.Omer
      alt[550] = 127;   // St.Pol
      alt[551] = 120;   // Fruges
      alt[552] = 105;   // Doullens
      alt[553] = 111;   // Frevent
      alt[554] = 133;   // Talmas
      alt[556] = 20;    // Amiens
      alt[561] = 27;    // Ardres
      alt[562] = 3;     // Gravelines
      alt[563] = 68;    // Lumbres
      alt[564] = 136;   // Escoeuilles
      alt[565] = 47;    // Hesdin
      alt[566] = 188;   // Maninghem
      alt[567] = 59;    // St.Ricquier
      alt[569] = 17;    // Fixecourt
      alt[572] = 90;    // Beauvais
      alt[573] = 44;    // Marquise
      alt[574] = 2;     // Wissant
      alt[575] = 5;     // Calais
      alt[576] = 82;    // Samer
      alt[577] = 5;     // Boulogne
      alt[578] = 50;    // Montreuil
      alt[579] = 6;     // Le Touquet
      alt[581] = 3;     // Le Crotoy
      alt[586] = 38;    // Bernay
      alt[589] = 8;     // Abbeville
      alt[592] = 2;     // Deal
      alt[593] = 1;     // Ramsgate
      alt[594] = 2;     // Berck-Plage
      alt[595] = 47;    // Canterbury
      alt[596] = 15;    // Wingham
      alt[597] = 2;     // Sandwich
      alt[598] = 2;     // Margate
      alt[599] = 2;     // Folkestone
      alt[600] = 2;     // Dover
      alt[601] = 130;   // Derringstone
      alt[602] = 46;    // Faversham
      alt[603] = 46;    // Whitstable
      alt[604] = 2;     // Eastchurch
      alt[605] = 76;    // Lympne
      alt[606] = 162;   // Ashford
      alt[607] = 34;    // Hornchurch
      alt[608] = 152;   // Biggin Hill
      alt[609] = 302;   // Losheim
      alt[610] = 424;   // Zerf
      alt[614] = 129;   // Trier
      alt[616] = 223;   // Saarburg
      alt[617] = 222;   // Bouzonville
      alt[618] = 169;   // Merzig
      alt[619] = 285;   // Raville
      alt[620] = 272;   // Boulay
      alt[621] = 232;   // Metzervisse
      alt[623] = 146;   // Sierck-les-Bains
      alt[624] = 227;   // Evrange
      alt[636] = 1;     // Dordrecht
      alt[637] = 1;     // Moerdijk
      alt[645] = 1;     // Den Haag
      alt[646] = 1;     // Spijkenisse
      alt[650] = 1;     // Cromstrijen
      alt[651] = 1;     // Westvoorne
      alt[656] = 1;     // Stellendam
      alt[657] = 1;     // Stavenisse
      alt[658] = 1;     // Brouwershaven
      alt[659] = 1;     // Ouddorp
      alt[660] = 1;     // Kats
      alt[661] = 11;    // Haamstede
      alt[662] = 1;     // Veere
      alt[663] = 1;     // Kamperland
      alt[664] = 1;     // Westkapelle
      alt[665] = 4;     // Coltishall
      alt[666] = 20;    // Martlesham
      alt[667] = 1;     // Ipswich
      alt[668] = 1;     // Brighton
      alt[681] = 54;    // Niederkassel
      alt[682] = 179;   // Meckenheim
      alt[683] = 120;   // Piesport
      alt[686] = 374;   // Musch
      alt[687] = 358;   // Kreuzberg
      alt[688] = 494;   // Daun
      alt[689] = 165;   // Wittlich
      alt[697] = 123;   // Schweich
      alt[698] = 382;   // Zemmer
      alt[701] = 155;   // Sommepy
      alt[702] = 111;   // Betheniville
      alt[703] = 47;    // Wickenby
      alt[704] = 260;   // Remscheid
      alt[705] = 103;   // Lohmar
      alt[706] = 69;    // Unkel
      alt[707] = 70;    // Remagen
      alt[711] = 186;   // Vohwinkel
      alt[712] = 180;   // Velbert
      alt[713] = 208;   // Overath
      alt[714] = 288;   // Kurten
      alt[715] = 206;   // Saarlouis
      alt[716] = 260;   // St.Avold
      alt[717] = 219;   // Creutzwald-la-Croix
      alt[718] = 260;   // Faulquemont
      alt[721] = 46;    // Chauny
      alt[724] = 65;    // Ham
      alt[726] = 38;    // Noyon
      alt[727] = 86;    // Chaulnes
      alt[728] = 63;    // Avricourt
      alt[729] = 84;    // Roye
      alt[730] = 81;    // Bouchoir
      alt[731] = 97;    // Moreuil
      alt[733] = 34;    // Picquigny
      alt[734] = 95;    // Essertaux
      alt[735] = 68;    // Airaines
      alt[737] = 176;   // Poix-de-Picardie
      alt[741] = 105;   // Oisemont
      alt[742] = 163;   // Liomer
      alt[743] = 1;     // Cayeux
      alt[744] = 94;    // Fressenneville
      alt[745] = 40;    // Gamaches
      alt[746] = 1;     // Le Treport
      alt[747] = 208;   // Foucarmont
      alt[748] = 113;   // Londinieres
      alt[749] = 1;     // Dieppe
      alt[750] = 1;     // Assigny
      alt[751] = 146;   // Evivermen
      alt[752] = 41;    // Woodchurch
      alt[753] = 1;     // Dungeness
      alt[754] = 6;     // Rye
      alt[755] = 1;     // Hastings
      alt[756] = 2;     // Winchelsea
      alt[757] = 1;     // Eastbourne
      alt[758] = 1;     // Seaford
      alt[759] = 91;    // Koblenz
      alt[760] = 246;   // Bad Sobernheim
      alt[763] = 98;    // Cochem
      alt[764] = 482;   // Ulmen
      alt[765] = 302;   // Kaisersesch
      alt[767] = 112;   // Bernkastel-Kues
      alt[770] = 112;   // Zell
      alt[772] = 511;   // Morbach
      alt[773] = 427;   // Hermeskeil
      alt[774] = 217;   // Clermont-en-Argonne
      alt[775] = 160;   // Suippes
      alt[776] = 133;   // Ste.Menehould
      alt[777] = 124;   // Mourmelon
      alt[781] = 98;    // Val-de-Vesle
      alt[785] = 22;    // Venray
      alt[786] = 268;   // Thiaucourt
      alt[787] = 24;    // Rips
      alt[788] = 223;   // Fresnes
      alt[789] = 4;     // Huesden
      alt[791] = 4;     // Empel
      alt[793] = 1;     // Dussen
      alt[794] = 1;     // Werkendam
      alt[800] = 115;   // Frankfurt

      return alt;
    }

    /// <summary>
    /// Gets rank data by parsing the embedded ranklist xml.
    /// </summary>
    /// <remarks>
    /// Ranklist provided by KFS1: http://ww2.kfs.org/ranklist.xml
    /// </remarks>
    /// <returns>An array of Rank objects.</returns>
    public static Rank[] GetRanks()
    {
      const int maxRank = 25;
      const int maxCountry = 4;
      const int maxBranch = 3;

      /* <ranklist>
       *   <rank level="1" type="enlisted">
		   *     <british country="1">
			 *       <ground branch="1">Recruit</ground>
			 *       <air branch="2">Aircraftsman</air>
			 *       <navy branch="3">Recruit</navy>
		   *     </british>
		   *     <french country="3">
			 *       ...
		   *     </french>
		   *     <german country="4">
			 *       ...
		   *     </german>
       *   </rank>
       *   ...
       * </ranklist>
       */

      XmlDocument xml = new XmlDocument();
      xml.XmlResolver = null;  // disable automatic dtd validation
      xml.LoadXml( Resources.ranklist );
      Rank[] rank = new Rank[maxRank + 1];

      foreach( XmlNode node in xml.SelectNodes( "/ranklist/rank" ) )
      {
        int id = int.Parse( node.Attributes["level"].Value );
        string type = node.Attributes["type"].Value;
        string[,] names = new string[maxCountry + 1, maxBranch + 1];

        foreach( XmlNode countryNode in node.ChildNodes )
        {
          int countryid = int.Parse( countryNode.Attributes["country"].Value );
          foreach( XmlNode branchNode in countryNode.ChildNodes )
          {
            int branchid = int.Parse( branchNode.Attributes["branch"].Value );
            names[countryid, branchid] = branchNode.InnerText;
          }
        }

        rank[id] = new Rank( id, type, names );
      }

      return rank;
    }

    /// <summary>
    /// Gets country border data by parsing embedded files of lat/long degree polygons.
    /// </summary>
    /// <remarks>
    /// Data provided by PSU's Digital Chart of the World Server based on NIMA's 'Vector
    /// Map Level 0' data. http://www.maproom.psu.edu/dcw/.
    /// </remarks>
    /// <returns>A list of CountryBorder's.</returns>
    public static List<CountryBorder> GetCountryBorders()
    {
      /* File format by line:
       * 
       *   CountryName   (refered to by Language.Enum_CountryName_...)
       *   <polygon number>
       *       <lat decimal degrees>  <long decimal degrees>    (polygon centroid, used to position country name)
       *       <lat decimal degrees>  <long decimal degrees>    (first point of polygon)
       *       ...etc...
       *   END
       *   <polygon number>
       *       <lat decimal degrees>  <long decimal degrees>    (polygon centroid, unused)
       *       <lat decimal degrees>  <long decimal degrees>    (first point of polygon)
       *       ...etc...
       *   END
       *   END    (end of file)
       */


      // get file contents to parse

      List<string> datafiles = new List<string> {
        Resources.border_belgium,
        Resources.border_france,
        Resources.border_germany,
        Resources.border_luxembourg,
        Resources.border_netherlands,
        Resources.border_uk
      };


      // loop over each datafile

      List<CountryBorder> borders = new List<CountryBorder>();
      foreach( string datafile in datafiles )
      {
        string countryName = null;
        PointF center = new PointF();
        List<PointF[]> polygons = new List<PointF[]>();
        List<PointF> polyBuffer = new List<PointF>();


        // tokenise file by splitting on whitespace

        string[] tokens = datafile.Split( new char[] {}, StringSplitOptions.RemoveEmptyEntries );
        for( int i = 0; i < tokens.Length - 1; i += 2 )
        {
          if( i == 0 )  // first token, store country name
          {
            countryName = tokens[i];
          }
          else if( tokens[i] == "END" )  // end token, flush buffer and start new polygon
          {
            PointF[] poly = new PointF[polyBuffer.Count];
            polyBuffer.CopyTo( poly );  // convert to array
            polygons.Add( poly );

            polyBuffer.Clear();

            i += 2;  // skip future centroid's
          }
          else  // degree pair token, parse and add to buffer
          {
            float x, y;
            try
            {
              x = float.Parse( tokens[i], CultureInfo.InvariantCulture );
              y = float.Parse( tokens[i + 1], CultureInfo.InvariantCulture );
            }
            catch( Exception ex )
            {
              throw new Exception( String.Format( "Failed to parse '{0}','{1}': {2}", tokens[i], tokens[i + 1], ex.Message ), ex );
            }

            if( center.IsEmpty )  // first cooord is polygon centroid
              center = new PointF( x, y );
            else
              polyBuffer.Add( new PointF( x, y ) );
          }
        }  // end foreach token


        // create new CountryBorder and add to list

        borders.Add( new CountryBorder( countryName, center, polygons ) );

      }  // end foreach datafile


      return borders;
    }
  }
}
