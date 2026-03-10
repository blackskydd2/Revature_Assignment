    using AutoMapper;
    using FluentValidation;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
   

    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomerController : ControllerBase
    {
        IMemoryCache _cache;
        ICustomerService customerService;
        IMapper mapper;

        IValidator<CreateCustomerDTO> createCustomerDTOValidator;

        public CustomerController(ICustomerService customerService, IMapper mapper, IValidator<CreateCustomerDTO> createCustomerDTOValidator, IMemoryCache cache)
        {
            this.customerService = customerService;
            this.mapper = mapper;
            this.createCustomerDTOValidator = createCustomerDTOValidator;
            this._cache = cache;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var customers = customerService.GetAllCustomers();

            // DRY -
            // Violation of Single Responsibility
            // Remove Manual mapping and replace with Automapper
            // var customerDTOs = customers.Select(c => new CustomerDTO
            // {
            //     FullName = c.Name
            // }).ToList();
            // Refactoring -

            // AutoMapper
            var customerDTOs = mapper.Map<List<CustomerDTO>>(customers);

            return Ok(customerDTOs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            // Caching
            var cacheKey = $"{id}";
            if (_cache.TryGetValue(cacheKey, out CustomerDTO? customerDTO))
            {
                return Ok(customerDTO);
            }

            await Task.Delay(5000); // Simulate a long-running operation

            var customer = customerService.GetAllCustomers().FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            customerDTO = mapper.Map<CustomerDTO>(customer);

            // Set cache options
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            // Save data in cache
            _cache.Set(cacheKey, customerDTO, cacheEntryOptions);

            return Ok(customerDTO);
        }


        [HttpPost]
        public IActionResult Post(CreateCustomerDTO createCustomerDTO)
        {

            var validationResult = createCustomerDTOValidator.Validate(createCustomerDTO);

            if (validationResult.IsValid == false)
            {
                return BadRequest(validationResult.Errors);
            }

            // if(ModelState.IsValid)
            // {
            //     // Map CreateCustomerDTO to Customer
            //     return BadRequest(ModelState);
            // }

            return Ok(createCustomerDTO);
        }
    }