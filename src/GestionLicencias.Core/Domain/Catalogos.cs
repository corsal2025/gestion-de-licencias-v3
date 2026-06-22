namespace GestionLicencias.Core.Domain;

/// <summary>
/// Listas de valores tomadas textualmente de las listas desplegables
/// de la planilla "MODELO POWERBI". No modificar sin actualizar el modelo.
/// </summary>
public static class Catalogos
{
    public static readonly string[] SiNo = ["SI", "NO"];

    public static readonly string[] LugaresAtencion =
    [
        "PLACILLA",
        "AV. ARGENTINA",
        "MERCADO PUERTO"
    ];

    public static readonly string[] EstadosCarpeta =
    [
        "1° LICENCIA",
        "SUBIDA A CONASET",
        "SUBIDA CON F8",
        "CAMBIO DOM. SUBIDO A CONASET",
        "CAMBIO DOM. SUBIDO CON CORREO",
        "SUBIDA CON OFICIO",
        "SE ENCUENTRA EN ARCHIVOS",
        "SE ENCUENTRA EN OF.43",
        "CAMBIO DE DOMICILIO SOLICITADO",
        "CAMBIO DE DOMICILIO",
        "NO EXISTE CARPETA",
        "CREAR OFICIO",
        "CANJE LIC. EXTRANJERA"
    ];

    // JUZGADO se muestra en rojo parpadeante; JUZGADO y REPROBADA detienen el
    // proceso (solo ADMINISTRADOR/DIRECTOR/JEFATURA pueden desbloquearlo).
    public static readonly string[] IdoneidadMoral = ["APROBADA", "REPROBADA", "JUZGADO"];

    public static readonly string[] CambioDomicilioPedidoPor =
    [
        "ALEXANDRA",
        "ANA",
        "LEONOR"
    ];

    public static readonly string[] EstadosF8 =
    [
        "SUBIDA A CONASET",
        "CREAR OFICIO",
        "CAMBIO DE DOMICILIO",
        "PRIMERA LICENCIA"
    ];

    public static readonly string[] Otorgamientos =
    [
        "OTORGADO",
        "DENEGADO",
        "ESPERA EXAMEN",
        "S/SGL"
    ];

    public static readonly string[] ImpresaPor =
    [
        "JOSELYN",
        "JAVIERA",
        "LISA",
        "SANDRA",
        "KARINA"
    ];

    public static readonly string[] RecibidaEnPamPor = ["SERGIO", "TAMARA"];

    // Extended catalogs (not part of the original spreadsheet)
    public static readonly string[] Sexos = ["MASCULINO", "FEMENINO"];

    /// <summary>
    /// Ciudades de Chile (norte a sur) para la columna CIUDADES de la sección
    /// Carpeta, habilitada cuando el estado es "CAMBIO DE DOMICILIO".
    /// </summary>
    public static readonly string[] CiudadesChile =
    [
        // Arica y Parinacota / Tarapacá
        "ARICA", "PUTRE", "IQUIQUE", "ALTO HOSPICIO", "POZO ALMONTE", "PICA",
        // Antofagasta
        "ANTOFAGASTA", "CALAMA", "TOCOPILLA", "MEJILLONES", "TALTAL",
        "SAN PEDRO DE ATACAMA", "MARÍA ELENA", "SIERRA GORDA",
        // Atacama
        "COPIAPÓ", "VALLENAR", "CALDERA", "CHAÑARAL", "DIEGO DE ALMAGRO",
        "TIERRA AMARILLA", "FREIRINA", "HUASCO", "ALTO DEL CARMEN",
        // Coquimbo
        "LA SERENA", "COQUIMBO", "OVALLE", "ILLAPEL", "VICUÑA", "ANDACOLLO",
        "COMBARBALÁ", "MONTE PATRIA", "PUNITAQUI", "SALAMANCA", "LOS VILOS",
        "PAIHUANO", "LA HIGUERA", "RÍO HURTADO", "CANELA",
        // Valparaíso
        "VALPARAÍSO", "VIÑA DEL MAR", "CONCÓN", "QUILPUÉ", "VILLA ALEMANA",
        "QUILLOTA", "LA CALERA", "SAN ANTONIO", "SAN FELIPE", "LOS ANDES",
        "LA LIGUA", "PETORCA", "CABILDO", "PAPUDO", "ZAPALLAR", "PUCHUNCAVÍ",
        "QUINTERO", "CASABLANCA", "LIMACHE", "OLMUÉ", "NOGALES", "HIJUELAS",
        "LA CRUZ", "CARTAGENA", "EL QUISCO", "EL TABO", "ALGARROBO",
        "SANTO DOMINGO", "LLAILLAY", "PANQUEHUE", "CATEMU", "PUTAENDO",
        "SANTA MARÍA", "CALLE LARGA", "RINCONADA", "SAN ESTEBAN",
        "ISLA DE PASCUA", "JUAN FERNÁNDEZ",
        // Metropolitana
        "SANTIAGO", "PUENTE ALTO", "MAIPÚ", "SAN BERNARDO", "MELIPILLA",
        "TALAGANTE", "PEÑAFLOR", "BUIN", "PAINE", "COLINA", "LAMPA", "TILTIL",
        "CURACAVÍ", "EL MONTE", "ISLA DE MAIPO", "PADRE HURTADO", "PIRQUE",
        "SAN JOSÉ DE MAIPO", "CALERA DE TANGO", "MARÍA PINTO", "ALHUÉ",
        "SAN PEDRO",
        // O'Higgins
        "RANCAGUA", "MACHALÍ", "GRANEROS", "MOSTAZAL", "RENGO", "REQUÍNOA",
        "SAN VICENTE DE TAGUA TAGUA", "PICHILEMU", "SAN FERNANDO",
        "SANTA CRUZ", "CHIMBARONGO", "NANCAGUA", "PALMILLA", "PERALILLO",
        "DOÑIHUE", "COLTAUCO", "CODEGUA", "OLIVAR", "COINCO", "QUINTA DE TILCOCO",
        "MALLOA", "PICHIDEGUA", "PEUMO", "LAS CABRAS", "LOLOL", "PUMANQUE",
        "CHÉPICA", "PLACILLA", "MARCHIGÜE", "LA ESTRELLA", "LITUECHE",
        "NAVIDAD", "PAREDONES",
        // Maule
        "TALCA", "CURICÓ", "LINARES", "CONSTITUCIÓN", "CAUQUENES", "MOLINA",
        "SAN CLEMENTE", "SAN JAVIER", "PARRAL", "LONGAVÍ", "COLBÚN",
        "VILLA ALEGRE", "YERBAS BUENAS", "TENO", "ROMERAL", "HUALAÑÉ",
        "LICANTÉN", "VICHUQUÉN", "RAUCO", "SAGRADA FAMILIA", "PELARCO",
        "RÍO CLARO", "MAULE", "EMPEDRADO", "PENCAHUE", "CUREPTO", "CHANCO",
        "PELLUHUE", "RETIRO",
        // Ñuble
        "CHILLÁN", "CHILLÁN VIEJO", "SAN CARLOS", "BULNES", "YUNGAY",
        "QUIRIHUE", "COELEMU", "COIHUECO", "ÑIQUÉN", "SAN FABIÁN",
        "SAN IGNACIO", "EL CARMEN", "PEMUCO", "PINTO", "QUILLÓN", "RÁNQUIL",
        "TREHUACO", "COBQUECURA", "NINHUE", "PORTEZUELO", "SAN NICOLÁS",
        // Biobío
        "CONCEPCIÓN", "TALCAHUANO", "HUALPÉN", "SAN PEDRO DE LA PAZ",
        "CHIGUAYANTE", "PENCO", "TOMÉ", "CORONEL", "LOTA", "LOS ÁNGELES",
        "ARAUCO", "CAÑETE", "CURANILAHUE", "LEBU", "LOS ÁLAMOS", "TIRÚA",
        "CONTULMO", "MULCHÉN", "NACIMIENTO", "LAJA", "SAN ROSENDO", "YUMBEL",
        "CABRERO", "TUCAPEL", "ANTUCO", "QUILLECO", "SANTA BÁRBARA",
        "QUILACO", "NEGRETE", "ALTO BIOBÍO", "FLORIDA", "HUALQUI",
        "SANTA JUANA",
        // La Araucanía
        "TEMUCO", "PADRE LAS CASAS", "ANGOL", "VILLARRICA", "PUCÓN",
        "VICTORIA", "LAUTARO", "NUEVA IMPERIAL", "LONCOCHE", "COLLIPULLI",
        "TRAIGUÉN", "CURACAUTÍN", "PURÉN", "LUMACO", "ERCILLA", "LOS SAUCES",
        "RENAICO", "LONQUIMAY", "GORBEA", "PITRUFQUÉN", "FREIRE", "CUNCO",
        "MELIPEUCO", "CURARREHUE", "TOLTÉN", "TEODORO SCHMIDT", "SAAVEDRA",
        "CARAHUE", "CHOLCHOL", "GALVARINO", "PERQUENCO", "VILCÚN",
        // Los Ríos
        "VALDIVIA", "LA UNIÓN", "RÍO BUENO", "PANGUIPULLI", "PAILLACO",
        "LOS LAGOS", "FUTRONO", "LANCO", "MÁFIL", "MARIQUINA", "CORRAL",
        "LAGO RANCO",
        // Los Lagos
        "OSORNO", "PUERTO MONTT", "PUERTO VARAS", "CASTRO", "ANCUD",
        "QUELLÓN", "CALBUCO", "LLANQUIHUE", "FRUTILLAR", "FRESIA",
        "PURRANQUE", "RÍO NEGRO", "SAN PABLO", "PUYEHUE", "PUERTO OCTAY",
        "SAN JUAN DE LA COSTA", "MAULLÍN", "LOS MUERMOS", "COCHAMÓ",
        "CHAITÉN", "FUTALEUFÚ", "PALENA", "HUALAIHUÉ", "DALCAHUE", "CHONCHI",
        "QUEILEN", "QUEMCHI", "ACHAO", "CURACO DE VÉLEZ", "PUQUELDÓN",
        // Aysén
        "COYHAIQUE", "PUERTO AYSÉN", "CHILE CHICO", "COCHRANE",
        "PUERTO CISNES", "VILLA O'HIGGINS", "LAGO VERDE", "GUAITECAS",
        "RÍO IBÁÑEZ", "TORTEL",
        // Magallanes
        "PUNTA ARENAS", "PUERTO NATALES", "PORVENIR", "PUERTO WILLIAMS",
        "PRIMAVERA", "TIMAUKEL", "TORRES DEL PAINE", "LAGUNA BLANCA",
        "RÍO VERDE", "SAN GREGORIO"
    ];

    public static readonly string[] TiposLicencia =
    [
        "A1", "A2", "A3", "A4", "A5",
        "B", "C", "D", "E", "F"
    ];
}
